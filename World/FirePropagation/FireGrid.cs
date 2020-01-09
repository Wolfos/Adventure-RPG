/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 10/01/2017
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public struct CellData
{
    public float HP;
    public float fuel;
    public float moisture;
    public float threshold;
    public float combustionValue;
    public float airTemperature;
    public float propagationSpeed;
    public float cellSize;
}


public class FireGrid : MonoBehaviour {
    [SerializeField][Tooltip("Prefab to be used for the fire.")]
    private GameObject m_firePrefab;
    private FireManager m_fireManager;
    private WindZone m_windZone;
    private Terrain m_terrain;
    private GameObject[,] m_cells;
    private SortedList<int, Vector2> m_alightCellIndex;
    private Vector3 m_origin; // terrain world origin
    private string m_terrianName;
    private float m_airTemperature = 0.0f;
    private float m_cellSize;
    [SerializeField][Tooltip("Number of cells in the gird (width).")]
    private int m_widthCells = 45;
    [SerializeField][Tooltip("Number of cells in the gird (height).")]
    private int m_heightCells = 45;
    private int m_allocatedListSize;
    private int m_DToAWidth;
    private int m_DToAHeight;
    private Vector3 m_propagationDirectionNorth;
    private Vector3 m_propagationDirectionEast;
    private Vector3 m_propagationDirectionSouth;
    private Vector3 m_propagationDirectionWest;
    private float m_bias;
    private float m_windBias;
    private float m_hillBias;
    private float m_maxHillDistance;
    private float m_visualThreshold;
    private float m_combustionRateValue;
    private bool m_day;
    private int m_width;
    private int m_height;
    private int m_fireCellsLit = 0;
    private bool m_fastSim;
    private bool m_centerCellIgnited = false;
    [SerializeField][Tooltip("Using a FireIgniter?")]
    private bool m_useIgniter = true;
    private bool m_gridCreated = false;
    private bool m_gridSlowBuild = true;

    void Start()
    {
        // If a tag was not set in the editor then fallback to slower why of finding the object
        try
        {
            GameObject manager = GameObject.FindWithTag("Fire");

            if (manager != null)
            {
                m_fireManager = manager.GetComponent<FireManager>();
            }
            else
            {
                // If a tag was not set in the editor then fallback to slower why of finding the object
                m_fireManager = FindObjectOfType<FireManager>();
                Debug.LogWarning("Fire Manager does not have the tag 'Fire'.");
            }
        }
        catch
        {
            // If a tag was not set in the editor then fallback to slower why of finding the object
            m_fireManager = FindObjectOfType<FireManager>();
            Debug.LogWarning("No 'Fire' tag set, looking for Fire Manager.");
        }

        if (m_fireManager != null)
        {
            // Get data from FireManager
            m_DToAWidth = m_fireManager.terrainWidth / m_fireManager.alphaWidth;
            m_DToAHeight = m_fireManager.terrainHeight / m_fireManager.alphaHeight;

            m_propagationDirectionNorth = new Vector3(0.0f, 0.0f, 1.0f);
            m_propagationDirectionEast = new Vector3(1.0f, 0.0f, 0.0f);
            m_propagationDirectionSouth = new Vector3(0.0f, 0.0f, -1.0f);
            m_propagationDirectionWest = new Vector3(-1.0f, 0.0f, 0.0f);

            // Only spawn if valid
            if (ValidIgnitionLocation())
            {
                // Get the rest of the data from FireManager
                m_windZone = m_fireManager.windzone;
                m_airTemperature = m_fireManager.airTemperature;
                m_cellSize = m_fireManager.cellSize;
                m_bias = m_fireManager.propagationBias;
                m_windBias = m_fireManager.propagationWindBias;
                m_hillBias = m_fireManager.propagationHillBias;
                m_maxHillDistance = m_fireManager.maxHillPropagationDistance;
                m_day = m_fireManager.daytime;
                m_visualThreshold = m_fireManager.visualExtinguishThreshold;
                m_combustionRateValue = m_fireManager.combustionRate;
                m_allocatedListSize = m_fireManager.preAllocatedFireIndexSize;
                m_terrianName = m_fireManager.terrain.name;
                m_fastSim = !m_fireManager.detailedSimulation; // invert, detailed == true, fast sim == false
                m_gridSlowBuild = m_fireManager.staggeredGridConstruction;

                // Increase the counter
                m_fireManager.AddActiveFireGrid();

                // FireGrassRemover is attached to FireGrid, normally by the FireIgniter
                GetComponentInParent<FireGrassRemover>().radius = m_cellSize;

                // If we are using an igniter, then the fire manager should be in the scene and already created. Otherwise we are starting a fire when the scene loads
                if (m_useIgniter == true)
                {
                    m_terrain = m_fireManager.terrain;
                }
                else
                {
                    // Get the terrain, needs to be a child of a Terrain GameObject
                    m_terrain = GetComponentInParent<Terrain>();
                }

                if (m_terrain != null)
                {
                    Vector3 terrainSize = m_terrain.terrainData.size;
                    m_origin = m_terrain.transform.position;

                    if (m_widthCells > (int)terrainSize.z)
                        m_widthCells = (int)terrainSize.z;

                    if (m_heightCells > (int)terrainSize.x)
                        m_heightCells = (int)terrainSize.x;

                    m_width = m_widthCells;
                    m_height = m_heightCells;

                    if(m_gridSlowBuild)
                    {
                        StartCoroutine(BuildGridStaged());
                    }
                    else
                    {
                        BuildGrid();
                    }
                }
                else
                {
                    Debug.LogError("Not a child of a Terrain GameObject!");
                }
            }
            else
            {
                // Invalid location so delete before the FireGrid is fully set up
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogWarning("No FireManager found in the scene!");
        }
    }

    void Update()
    {
        // Requires a valid fire manager
        if (m_fireManager != null && m_gridCreated)
        {
            // Only do this once when the fire starts in the grid
            if (m_centerCellIgnited == false)
            {
                // Start fire
                Vector2 ArrayCenter = new Vector2((float)m_cells.GetLength(0) / 2.0f, (float)m_cells.GetLength(1) / 2.0f);
                m_cells[(int)ArrayCenter.x, (int)ArrayCenter.y].GetComponent<FireCell>().ignitionTemperature = 0.0f;
                m_cells[(int)ArrayCenter.x, (int)ArrayCenter.y].GetComponent<FireCell>().HeatsUp();
                m_centerCellIgnited = true;
            }
        
            // Fast or detailed simulation
            if (m_fastSim)
            {
                // Is wind affecting fire propagation
                if (m_windZone != null)
                {
                    // Get the wind direction and perform a dot product with the propagation direction. 
                    // Results: >= 0 propagation in same direction as wind, < 0 propgation going against wind
                    Quaternion windQuat = m_windZone.transform.rotation;
                    Vector3 windDirection = new Vector3(0.0f, 0.0f, 1.0f);
                    windDirection = windQuat * windDirection;

                    // Propgation to cells around the fire, if a cell is on fire
                    FastPropagation(windDirection.normalized);
                }
                else
                {
                    Propagation();
                }
            }
            else
            {
                // Is wind affecting fire propagation
                if (m_windZone != null)
                {
                    // Get the wind direction and perform a dot product with the propagation direction. 
                    // Results: >= 0 propagation in same direction as wind, < 0 propgation going against wind
                    Quaternion windQuat = m_windZone.transform.rotation;
                    Vector3 windDirection = new Vector3(0.0f, 0.0f, 1.0f);
                    windDirection = windQuat * windDirection;
                    windDirection = windDirection.normalized;

                    // Propgation to cells around the fire, if a cell is on fire
                    Propagation(windDirection);
                }
                else
                {
                    Propagation();
                }
            }
            // Ensures that the FireGrid is deleted after no more fires have been create or currently lit
            if (m_fireCellsLit == 0)
                Destroy(gameObject, 2);
        }
    }

    void OnDestroy()
    {
        if (m_fireManager != null)
        {
            // Decrease the counter
            m_fireManager.RemoveActiveFireGrid();
        }
    }

    // brief Call this after a new FireGrid script component is created (i.e. called from FireIgniter)
    // param GameObject The prefab that should be used to create fires
    // param Vector3 The position that will be the center of the grid
    // param int Width of the grid
    // param int Height of the grid
    public void IgniterUpdate(GameObject firePrefab, Vector3 position, int gridWidth, int gridHeight)
    {
        transform.position = position;
        m_widthCells = gridWidth;
        m_heightCells = gridHeight;
        m_firePrefab = firePrefab;
    }

    // brief Build the grid over many frames, good for large sized grids
    // Return IEnumerator as is a CoRoutine
    IEnumerator BuildGridStaged()
    {
        m_alightCellIndex = new SortedList<int, Vector2>(m_allocatedListSize);

        float offsetX = 0.0f;
        float offsetY = 0.0f;

        // get the offset, depending if it is an even or odd width or height
        if (m_width % 2 == 0)
            offsetX = (m_width / 2.0f) * m_cellSize;
        else if (m_width % 2 == 1)
            offsetX = ((m_width - 1) / 2.0f) * m_cellSize;

        if (m_height % 2 == 0)
            offsetY = (m_height / 2.0f) * m_cellSize;
        else if (m_height % 2 == 1)
            offsetY = ((m_height - 1) / 2.0f) * m_cellSize;

        m_cells = new GameObject[m_width, m_height];
        GameObject tmp = new GameObject();
        tmp.AddComponent<FireCell>();
        Quaternion quat = new Quaternion();

        // create cell data of common data
        CellData cellData;
        cellData.airTemperature = m_airTemperature;
        cellData.threshold = m_visualThreshold;
        cellData.combustionValue = m_combustionRateValue;
        cellData.cellSize = m_cellSize;

        yield return null;

        // create the cells in the grid
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                Vector2 index = new Vector2((transform.position.x - offsetX) + (x * m_cellSize), (transform.position.z - offsetY) + (y * m_cellSize));
                Vector3 worldPosition = GetWorldPosition(index);
                worldPosition.y = m_terrain.SampleHeight(worldPosition) + 0.001f;

                m_cells[x, y] = (GameObject)Instantiate(tmp, worldPosition, quat, transform);
                m_cells[x, y].name = "FireCell " + (x * m_width + y).ToString();
                FireCell firecell = m_cells[x, y].GetComponent<FireCell>();

                cellData.propagationSpeed = GetValuesFromFuelType((int)worldPosition.x, (int)worldPosition.z, out cellData.HP, out cellData.fuel, out cellData.moisture);
                firecell.SetupCell(false, m_firePrefab, cellData, m_terrianName, m_fireManager.cellFireSpawnPositions);

                // modify the heat of the fire, to make the fire burn slower the greater the distance the fire is from the start position
                float baisDistance = Vector3.Distance(firecell.position, transform.position);
                baisDistance /= m_bias;

                // fire temp could still be modified to minus value by the wind in the propagation step
                firecell.fireTemperature -= baisDistance;
            }
            yield return null;
        }

        DestroyImmediate(tmp);
        m_gridCreated = true;
    }

    // brief Build a grid in the a single frame, good for small sized grids
    void BuildGrid()
    {
        m_alightCellIndex = new SortedList<int, Vector2>(m_allocatedListSize);

        float offsetX = 0.0f;
        float offsetY = 0.0f;

        // Get the offset, depending if it is an even or odd width or height
        if (m_width % 2 == 0)
            offsetX = (m_width / 2.0f) * m_cellSize;
        else if (m_width % 2 == 1)
            offsetX = ((m_width - 1 ) / 2.0f) * m_cellSize;

        if (m_height % 2 == 0)
            offsetY = (m_height / 2.0f) * m_cellSize;
        else if (m_height % 2 == 1)
            offsetY = ((m_height - 1) / 2.0f) * m_cellSize;

        m_cells = new GameObject[m_width, m_height];
        GameObject tmp = new GameObject();
        tmp.AddComponent<FireCell>();
        Quaternion quat = new Quaternion();

        // Create cell data of common data
        CellData cellData;
        cellData.airTemperature = m_airTemperature;
        cellData.threshold = m_visualThreshold;
        cellData.combustionValue = m_combustionRateValue;
        cellData.cellSize = m_cellSize;

        // Create the cells in the grid
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                Vector2 index = new Vector2((transform.position.x - offsetX) + (x * m_cellSize), (transform.position.z - offsetY) + (y * m_cellSize));
                Vector3 worldPosition = GetWorldPosition(index);
                worldPosition.y = m_terrain.SampleHeight(worldPosition) + 0.001f;

                m_cells[x, y] = (GameObject)Instantiate(tmp, worldPosition, quat, transform);
                m_cells[x, y].name = "FireCell " + (x * m_width + y).ToString();
                FireCell firecell = m_cells[x, y].GetComponent<FireCell>();

                cellData.propagationSpeed = GetValuesFromFuelType((int)worldPosition.x, (int)worldPosition.z, out cellData.HP, out cellData.fuel, out cellData.moisture);
                firecell.SetupCell(false, m_firePrefab, cellData, m_terrianName, m_fireManager.cellFireSpawnPositions);

                // Modify the heat of the fire, to make the fire burn slower the greater the distance the fire is from the start position
                float baisDistance = Vector3.Distance(firecell.position, transform.position);
                baisDistance /= m_bias;

                // Fire temp could still be modified to minus value by the wind in the propagation step
                firecell.fireTemperature -= baisDistance;
            }
        }

        DestroyImmediate(tmp);
        m_gridCreated = true;
    }

    // brief Gets the world position of the grid
    // param Vector2 Grid position
    // Returns Vector3 Grid position in world
    public Vector3 GetWorldPosition(Vector2 gridPosition)
    {
        return new Vector3(m_origin.z + (gridPosition.x), m_origin.y, m_origin.x + (gridPosition.y));
    }

    // brief Gets the grids' position in the world
    // param Vector3 Grid position in world (3D)
    // Returns Vector2 Grid position (2D)
    public Vector2 GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2(worldPosition.z / m_cellSize, worldPosition.x / m_cellSize);
    }

    // brief Propagates the fire within this grid taking wind into account
    // param Vector3 Wind direction
    void FastPropagation(Vector3 windDirection)
    {
        m_fireCellsLit = 0;

        // Keep within grid boundary for the next step
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                // Find out if the key is in the list, if not add it otherwise remove it once the fire has ended
                Vector2 indexVector = new Vector2(x, y);
                int index = x * m_width + y;
                bool contained = m_alightCellIndex.ContainsKey(index);

                if (m_cells[x, y].GetComponent<FireCell>().fireProcessHappening && contained == false)
                    m_alightCellIndex.Add(index, indexVector);
                else if (contained == true)
                    m_alightCellIndex.Remove(index);
            }
        }

        FireGrassRemover grassRemover = GetComponentInParent<FireGrassRemover>();

        foreach (Vector2 index in m_alightCellIndex.Values)
        {
            int x = (int)index.x;
            int y = (int)index.y;

            FireCell firecell = m_cells[x, y].GetComponent<FireCell>();
            firecell.GridUpdate(grassRemover);

            if (firecell.isAlight)
            {
                // Keep in array bounds
                if (x < m_width - 1)
                {
                    // Is wind affecting the propgation in this direction
                    if (Vector3.Dot(windDirection, m_propagationDirectionEast.normalized) >= 0.0f)
                    {
                        FireCell selectedCell = m_cells[x + 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // if fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        FireCell selectedCell = m_cells[x + 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                        {
                            selectedCell.fireTemperature *= ComputeHeadWind();
                            selectedCell.temperatureModified = true;
                            selectedCell.HeatsUp();
                        }
                    }
                }

                // Keep in array bounds
                if (y < m_height - 1)
                {
                    if (Vector3.Dot(windDirection, m_propagationDirectionNorth.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        FireCell selectedCell = m_cells[x, y + 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        FireCell selectedCell = m_cells[x, y + 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                        {
                            selectedCell.fireTemperature *= ComputeHeadWind();
                            selectedCell.temperatureModified = true;
                            selectedCell.HeatsUp();
                        }
                    }
                }

                // Keep in array bounds
                if (x > 0)
                {
                    if (Vector3.Dot(windDirection, m_propagationDirectionWest.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        FireCell selectedCell = m_cells[x - 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        FireCell selectedCell = m_cells[x - 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                        {
                            selectedCell.fireTemperature *= ComputeHeadWind();
                            selectedCell.temperatureModified = true;
                            selectedCell.HeatsUp();
                        }
                    }
                }

                // Keep in array bounds
                if (y > 0)
                {
                    if (Vector3.Dot(windDirection, m_propagationDirectionSouth.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        FireCell selectedCell = m_cells[x, y - 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        FireCell selectedCell = m_cells[x, y - 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                        {
                            selectedCell.fireTemperature *= ComputeHeadWind();
                            selectedCell.temperatureModified = true;
                            selectedCell.HeatsUp();
                        }
                    }
                }
            }
        }

        // Number of active fires
        m_fireCellsLit = m_alightCellIndex.Count;
    }

    // brief Propagates the fire within this grid taking wind direction into account
    // param Vector3 Wind direction
    void Propagation(Vector3 windDirection)
    {
        m_fireCellsLit = 0;

        // Keep within grid boundary for the next step
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                Vector2 indexVector = new Vector2(x, y);
                int index = x * m_width + y;
                bool contained = m_alightCellIndex.ContainsKey(index);

                if (m_cells[x, y].GetComponent<FireCell>().fireProcessHappening && contained == false)
                    m_alightCellIndex.Add(index, indexVector);
                else if (contained == true)
                    m_alightCellIndex.Remove(index);
            }
        }

        foreach(Vector2 index in m_alightCellIndex.Values)
        {
            int x = (int)index.x;
            int y = (int)index.y;

            FireCell firecell = m_cells[x, y].GetComponent<FireCell>();
            firecell.GridUpdate(GetComponentInParent<FireGrassRemover>());

            if (firecell.isAlight)
            {
                // Keep in array bounds
                if (x < m_width - 1)
                {
                    // Is wind affecting the propgation in this direction
                    if (Vector3.Dot(windDirection, m_propagationDirectionEast.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x + 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                // Propagating in the same direction as the wind, so don't need to reduce fire temperature
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x + 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                if (selectedCell.temperatureModified == false)
                                {
                                    float bias = Mathf.Abs(ComputeHeadWind() - m_bias);
                                    selectedCell.fireTemperature *= bias;
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Up hill
                            case 1:
                                if (m_cells[x + 1, y].GetComponent<FireCell>().temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWindBias());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    // If it is day, fire should be mostly propagating up the hill so decrease the fire temperature for fire propagating downhill
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }


                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                }

                // Keep in array bounds
                if (y < m_height - 1)
                {
                    if (Vector3.Dot(windDirection, m_propagationDirectionNorth.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x, y + 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                // Propagating in the same direction as the wind, so don't need to reduce fire temperature
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x, y + 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                if (m_cells[x, y + 1].GetComponent<FireCell>().temperatureModified == false)
                                {
                                    float bias = Mathf.Abs(ComputeHeadWind() - m_bias);
                                    selectedCell.fireTemperature *= bias;
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWindBias());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    // If it is day, fire should be mostly propagating up the hill so decrease the fire temperature for fire propagating downhill
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }


                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                }

                // Keep in array bounds
                if (x > 0)
                {
                    if (Vector3.Dot(windDirection, m_propagationDirectionWest.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x - 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                // Propagating in the same direction as the wind, so don't need to reduce fire temperature
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x - 1, y].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                if (selectedCell.temperatureModified == false)
                                {
                                    float bias = Mathf.Abs(ComputeHeadWind() - m_bias);
                                    selectedCell.fireTemperature *= bias;
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWindBias());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    // If it is day, fire should be mostly propagating up the hill so decrease the fire temperature for fire propagating downhill
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }


                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                }

                // Keep in array bounds
                if (y > 0)
                {
                    if (Vector3.Dot(windDirection, m_propagationDirectionSouth.normalized) >= 0.0f)
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x, y - 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                // Propagating in the same direction as the wind, so don't need to reduce fire temperature
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    else
                                    {
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() + ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias; // Modify temperature
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }

                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                    else // Going against wind/into head on winds
                    {
                        // Is the fire propagating on a slope, if so which direction and time of day
                        // during day time hot air rises, meaning the fire will propagate uphill faster then downhill
                        FireCell selectedCell = m_cells[x, y - 1].GetComponent<FireCell>();
                        int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                        // Apply slope affects
                        switch (slopeCode)
                        {
                            // Flat
                            case 0:
                                if (selectedCell.temperatureModified == false)
                                {
                                    float bias = Mathf.Abs(ComputeHeadWind() - m_bias);
                                    selectedCell.fireTemperature *= bias;
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Up hill
                            case 1:
                                if (selectedCell.temperatureModified == false)
                                {
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWindBias());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                            // Down hill
                            case 2:
                                if (selectedCell.temperatureModified == false)
                                {
                                    // If it is day, fire should be mostly propagating up the hill so decrease the fire temperature for fire propagating downhill
                                    if (m_day)
                                    {
                                        // As it day, using the down slope bias (will reduce temp) rather then up slope bias (will increase temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeUpSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    else
                                    {
                                        // As it night, using the up slope bias (will increase temp) rather then down slope bias (will decrease temp - we don't want this)
                                        float bias = Mathf.Abs(ComputeDownSlopeBias() - ComputeHeadWind());
                                        selectedCell.fireTemperature *= bias;
                                    }
                                    selectedCell.temperatureModified = true;
                                }
                                break;
                        }


                        // If fire can propagate to, heat up the grid
                        if (slopeCode != -1)
                            selectedCell.HeatsUp();
                    }
                }
            }
        }

        // Number of active fires
        m_fireCellsLit = m_alightCellIndex.Count;
    }

    // brief Propagates the fire within this grid without wind
    void Propagation()
    {
        m_fireCellsLit = 0;

        // Keep within grid boundary for the next step
        for (int x = 0; x < m_width; x++)
        {
            for (int y = 0; y < m_height; y++)
            {
                Vector2 indexVector = new Vector2(x, y);
                int index = x * m_width + y;
                bool contained = m_alightCellIndex.ContainsKey(index);

                if (m_cells[x, y].GetComponent<FireCell>().fireProcessHappening && contained == false)
                    m_alightCellIndex.Add(index, indexVector);
                else if (contained == true)
                    m_alightCellIndex.Remove(index);
            }
        }

        foreach (Vector2 index in m_alightCellIndex.Values)
        {
            int x = (int)index.x;
            int y = (int)index.y;

            FireCell firecell = m_cells[x, y].GetComponent<FireCell>();
            firecell.GridUpdate(GetComponentInParent<FireGrassRemover>());

            if (firecell.isAlight)
            {
                // Keep in array bounds
                if (x < m_width - 1)
                {
                    FireCell selectedCell = m_cells[x + 1, y].GetComponent<FireCell>();
                    int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                    // If fire can propagate to, heat up the grid
                    if (slopeCode != -1)
                        selectedCell.HeatsUp();
                }

                // Keep in array bounds
                if (y < m_height - 1)
                {
                    // Is the fire propagating on a slope, if so which direction and time of day
                    FireCell selectedCell = m_cells[x, y + 1].GetComponent<FireCell>();
                    int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                    // If fire can propagate to, heat up the grid
                    if (slopeCode != -1)
                        selectedCell.HeatsUp();
                }

                // Keep in array bounds
                if (x > 0)
                {
                    // Is the fire propagating on a slope, if so which direction and time of day
                    FireCell selectedCell = m_cells[x - 1, y].GetComponent<FireCell>();
                    int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                    // If fire can propagate to, heat up the grid
                    if (slopeCode != -1)
                        selectedCell.HeatsUp();
                }

                // Keep in array bounds
                if (y > 0)
                {
                    // Is the fire propagating on a slope, if so which direction and time of day
                    FireCell selectedCell = m_cells[x, y - 1].GetComponent<FireCell>();
                    int slopeCode = PropagatingOnSlope(firecell.position, selectedCell.position);

                    // If fire can propagate to, heat up the grid
                    if (slopeCode != -1)
                        selectedCell.HeatsUp();
                }
            }
        }

        // Number of active fires
        m_fireCellsLit = m_alightCellIndex.Count;
    }

    // brief Gets the fire propagation speed based on the terrain fuel type the cell is located on, also fuel, mositure and HP values
    // param int Cell X position
    // param int Cell Y position
    // return float cell Hit Points
    // return float cell fuel
    // return float cell mositure
    float GetValuesFromFuelType(int x, int y, out float hp, out float fuel, out float mositure)
    {
        float speed = 0.0f;
        float hpValue = 0.0f;
        float fuelValue = 0.0f;
        float mositureValue = 0.0f;
        int counter = 0;

        // By default the alphamap is 512x512 and ther terrain is 1024x1024 so need to work out the scale difference
        int X = 0;
        int Y = 0;

        // Keep within alpha map array bounds, quick exit
        if (x < 0 || x >= m_fireManager.alphaWidth)
        {
            x = 0;
            hp = 0;
            fuel = 0;
            mositure = 0;
            return speed;
        }
        if (y < 0 || y >= m_fireManager.alphaHeight)
        {
            x = 0;
            hp = 0;
            fuel = 0;
            mositure = 0;
            return speed;
        }

        // Getting the correct point from the terrain
        Vector3 texturePoint3D = new Vector3(x, 0.0f, y);
        texturePoint3D = texturePoint3D * m_fireManager.terrainDetailSize;
        float[] m_pixelPoints = new float[4];
        m_pixelPoints[0] = texturePoint3D.z + 1.0f;
        m_pixelPoints[1] = texturePoint3D.z - 1.0f;
        m_pixelPoints[2] = texturePoint3D.x + 1.0f;
        m_pixelPoints[3] = texturePoint3D.x - 1.0f;
        int alphaLen = m_fireManager.terrainAlpha.GetLength(2);

        // Find which point on the alphamap the fire is on
        for (int yy = (int)m_pixelPoints[3]; yy < (int)m_pixelPoints[2]; yy++)
        {
            for (int xx = (int)m_pixelPoints[1]; xx < (int)m_pixelPoints[0]; xx++)
            {
                X = xx / m_DToAWidth;
                Y = yy / m_DToAHeight;

                // Keep within array bounds
                if (X < 0)
                    X = 0;
                else if (X >= m_fireManager.alphaWidth)
                    X = m_fireManager.alphaWidth - 1;
                if (Y < 0)
                    Y = 0;
                else if (Y >= m_fireManager.alphaHeight)
                    Y = m_fireManager.alphaHeight - 1; 

                // Alphamaps are 3D arrays, x,y are the x/y coordinate position and the 3rd is the splatmap
                // for each splatmap (texture) the terrain has, find if the texture is on the terrain and if it is flammable
                for (int i = 0; i < alphaLen; i++)
                {
                    int result = (int)m_fireManager.terrainAlpha[X, Y, i];          
                
                    // If the texture is not flammable, set all values to 0 to stop any fire from starting, otherwise compute cell values
                    if(m_fireManager.terrainTextures[i].m_flammable == false && result > 0)
                    {
                        speed = 0.0f;
                        hpValue = 0.0f;
                        fuelValue = 0.0f;
                        mositureValue = 0.0f;
                    }
                    else
                    {
                        // Need to convert terrain position to alphamap position
                        float posValue = m_fireManager.terrainAlpha[x, y, i];
                
                        if (posValue > 0.0f)
                        {
                            speed += m_fireManager.terrainTextures[i].propagationSpeed;
                            hpValue = m_fireManager.terrainTextures[i].CellHP();
                            fuelValue = m_fireManager.terrainTextures[i].CellFuel();
                            mositureValue = m_fireManager.groundMoisture + m_fireManager.terrainTextures[i].CellMoisture(); // global moisture + cell local moisture
                            counter++;
                        }
                    }
                }
            }
        }

        if (counter != 0)
            speed /= counter;

        hp = hpValue;
        fuel = fuelValue;
        mositure = mositureValue;
        return speed;
    }

    // brief Returns if a location is a valid location to start a fire
    // return True if valid location to spawn fire otherwise false
    bool ValidIgnitionLocation()
    {
        bool valid = false;

        // By default the alphamap is 512x512 and ther terrain is 1024x1024 so need to work out the scale difference
        int X = 0;
        int Y = 0;

        // Getting the correct point from the terrain
        Vector3 texturePoint3D = new Vector3(transform.position.x, 0.0f, transform.position.z);
        texturePoint3D = texturePoint3D * m_fireManager.terrainDetailSize;
        float[] m_pixelPoints = new float[4];
        m_pixelPoints[0] = texturePoint3D.z + 1.0f;
        m_pixelPoints[1] = texturePoint3D.z - 1.0f;
        m_pixelPoints[2] = texturePoint3D.x + 1.0f;
        m_pixelPoints[3] = texturePoint3D.x - 1.0f;
        int alphaLen = m_fireManager.terrainAlpha.GetLength(2);


        // Keep within array bounds
        for (int i = 0; i < 4; i++)
        {
            if (m_pixelPoints[i] < 0)
                m_pixelPoints[i] = 0;

            if (m_pixelPoints[i] > m_fireManager.alphaHeight || m_pixelPoints[i] > m_fireManager.alphaWidth)
                m_pixelPoints[i] = m_fireManager.alphaHeight - 1; // Height and Width should always be the same, checked on creation of the grid.

        }

        // Find which point on the alphamap the fire is on
        for (int yy = (int)m_pixelPoints[3]; yy < (int)m_pixelPoints[2] + 1; yy++)
        {
            for (int xx = (int)m_pixelPoints[1]; xx < (int)m_pixelPoints[0] + 1; xx++)
            {
                X = xx / m_DToAWidth;
                Y = yy / m_DToAHeight;
                
                // Alphamaps are 3D arrays, x,y are the x/y coordinate position and the 3rd is the splatmap
                // for each splatmap (texture) the terrain has, find if the texture is on the terrain and if it is flammable
                for (int i = 0; i < alphaLen; i++)
                {
                    int result = (int)m_fireManager.terrainAlpha[X, Y, i];
                
                    // If the texture is not flammable, set all values to 0 to stop any fire from starting, otherwise compute cell values
                    if (m_fireManager.terrainTextures[i].m_flammable == false && result > 0)
                    {
                        valid = false;
                    }
                    else
                    {
                        // Need to convert terrain position to alphamap position
                        float posValue = m_fireManager.terrainAlpha[(int)transform.position.x, (int)transform.position.z, i];
                
                        if (posValue > 0.0f)
                        {
                            valid = true;
                        }
                    }
                
                }
            }
        }

        return valid;
    }

    // brief Computes the bias value of a head wind
    // return float The bias value when fire is affected by propagating into head on winds, clamped between 1.0f -> max value
    float ComputeHeadWindBias()
    {
        float result = m_windZone.windMain + m_windBias;
        return Mathf.Clamp(result, 1.0f, float.MaxValue);
    }

    // brief Computes the head wind strength
    // return float The head wind strength
    float ComputeHeadWind()
    {
        return m_windZone.windMain * m_windBias;
    }

    // brief Computes the wind bais up slope
    // return float The bias value when affected by a up hill slope
    float ComputeUpSlopeBias()
    {
        return m_hillBias + m_bias;
    }

    // brief Computes the wind bais down slope
    // return float the bias value when affected by a down hill slope
    float ComputeDownSlopeBias()
    {
        return m_hillBias - m_bias;
    }

    // brief Is the fire propagating on a slope, if so which way (up or down) or is it possiable to propagate (i.e. slope to steep/cliff)
    // param Vector3 Fire position
    // param Vector3 Position the fire wants to propagate to
    // return int:
    // 0 = flat
    // 1 = up hill
    // 2 = down hill
    // -1 = cannot propagate to as exceeds max distance
    int PropagatingOnSlope(Vector3 fireOrigin, Vector3 fireTarget)
    {
        // the distance will be the same as the cell size if propagating on flat terrain
        float distance = Vector3.Distance(fireOrigin, fireTarget);

        // make sure the fire cannot propagate over define threshold, so it cannot propagate up/down a cliff for example.
        if (distance <= m_maxHillDistance)
        {
            // is the fire propagating up for down hill?
            if (fireTarget.y > fireOrigin.y) // up hill
            {
                return 1;
            }
            else if (fireTarget.y < fireOrigin.y) // down hill
            {
                return 2;
            }
            else // flat
            {
                return 0;
            }
        }
        else if (Mathf.Abs(fireOrigin.y - fireTarget.y) < m_maxHillDistance)
        {
            return 0; // flat
        }

        return -1; // cannot propagate to target
    }
}
