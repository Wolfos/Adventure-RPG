/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 10/02/2017
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireManager : MonoBehaviour {
    private Terrain m_terrain = null; // terrain should be the parent GameObject of this object
    [SerializeField][Tooltip("The Windzone to be used for the simulation.")]
    private WindZone m_windZone;
    [SerializeField][Tooltip("Size of the index array used to keep track of active fires in a FireGrid, if goes over array will increase.")]
    private int m_preAllocatedFireIndexSize = 65;
    [SerializeField][Tooltip("Air temperature in the scene, the higher the value fuels heat up quicker.")]
    private float m_airTemperature = 0.0f;
    [SerializeField][Tooltip("The amount of ground moisture in the scene, higher the value the slower cells heat up.")]
    private float m_groundMoisture = 0.0f;
    [SerializeField][Tooltip("Size of the cell.")]
    private float m_cellSize = 1.0f;
    [SerializeField][Tooltip("Relative position of fire spawning in the cell (0 -> 1). If left empty default value is 0.5 on X and Y.")]
    private Vector2[] m_cellFireSpawnPositions;
    [SerializeField][Tooltip("How fast fuel is used in the combustion step within FireCell's, higher the value the faster fuels are used up.")]
    private float m_combustionRate = 5.0f;
    [SerializeField][Tooltip("How fast fuel is used in the combustion step with FireNode's, higher the value the faster fuels are used up.")]
    private float m_nodeCombustionRate = 5.0f;
    [SerializeField][Tooltip("Smaller the value the more random/less uniformed the propagation.")]
    private float m_propagationBias = 0.4f;
    [SerializeField][Tooltip("Larger the value the faster propagation is up/down hills")]
    private float m_propagationHillBias = 1.0f;
    [SerializeField][Tooltip("Larger the value the faster fire can propagate into the wind, Windzone force can also affect this.")]
    private float m_propagationWindBias = 0.99f;
    [SerializeField][Tooltip("The largest height distance fire can propagate on terrain. Stops fire propagating up large cliffs.")]
    private float m_maxHillPropagationDistance = 0.6f;
    [SerializeField][Tooltip("At what point the extinguish particle systems should be active, is a percentage (0 -> 1). Doesn't affect simulation, only visuals.")]
    private float m_visualExtinguishThreshold = 0.2f;
    [SerializeField][Tooltip("True for day.")]
    private bool m_dayTime = true;
    [SerializeField][Tooltip("The textured used on the terrain are fuels, can set fuel values for each texture.")]
    private FireTerrainTexture[] m_terrainTextures;
    [SerializeField][Tooltip("The index of the terrain grass texture to replace grass with once burnt, this is used only if 'Remove Grass Once Burnt' is disabled.")]
    private int m_burntGrassTextureIndex = 0;
    [SerializeField][Tooltip("Enable detailed fire propagation simulation, this may have a small impact on performance!")]
    private bool m_detailedSimulation = false;
    [SerializeField][Tooltip("Removes grass in a lit fire cell.")]
    private bool m_removeGrassOnceBurnt = false;
    [SerializeField][Tooltip("Replaced ground textures with scorch marks, this may have a small impact on performance!")]
    private bool m_replaceGroundTextureOnceBurnt = true;
    [SerializeField][Tooltip("The about of time taken before the next terrain update.")]
    private float m_terrainUpdateTime = 1.0f;
    [SerializeField][Tooltip("Fire Grid's are constructed over several frames, good when using larger sized grids.")]
    private bool m_staggeredGridConstruction = true;
    [SerializeField][Tooltip("Use as many terrain grass texture details as possible, this may have an impact on performance!")]
    private bool m_maxGrassDetails = false;
    [SerializeField][Tooltip("Which index of the grass detail texture is the burnt grass, only used if 'Max Grass Details' is enabled.")]
    private int m_burntGrassDetailIndex = 1;
    private float m_terrainUpdateTimer = 0.0f; // update timer
    private List<int[,]> m_terrainMaps;
    private List<int[,]> m_terrainMapsOriginal;
    private int[,] m_terrainMap; // used for grass and removing grass (normal)
    private int[,] m_terrainMapOriginal; // used for grass and removing grass (normal)
    private int[,] m_terrainReplaceMap; // used for grass and removing grass (normal)
    private int[,] m_terrainReplaceMapOriginal; // used for grass and removing grass (normal)
    private float[,,] m_terrainTexture; // used for replacing terrain textures
    private float[,,] m_terrainTextureOriginal; // used for replacing terrain textures
    private float m_terrainDetailSize;
    private int m_terrainDetailWidth;
    private int m_terrainDetailHeight;
    private int m_terrainAlphaWidth;
    private int m_terrainAlphaHeight;
    private bool m_dirty = false;
    private int m_activeFireGrids = 0;

    public WindZone windzone 
    {
        get { return m_windZone;  }
        set { m_windZone = value;  }
    }
    public int preAllocatedFireIndexSize
    {
        get { return m_preAllocatedFireIndexSize; }
        set { m_preAllocatedFireIndexSize = value; }
    }
    public float airTemperature
    {
        get { return m_airTemperature; }
        set { m_airTemperature = value; }
    }
    public float groundMoisture
    {
        get { return m_groundMoisture; }
        set { m_groundMoisture = value; }
    }
    public float cellSize
    {
        get { return m_cellSize; }
        set { m_cellSize = value; }
    }
    public List<int[,]> terrainMaps
    {
        get { return m_terrainMaps; }
        set { m_terrainMaps = value; }
    }
    public int[,] terrainMap
    {
        get { return m_terrainMap; }
        set { m_terrainMap = value; }
    }
    public int[,] terrainReplacementMap
    {
        get { return m_terrainReplaceMap; }
        set { m_terrainReplaceMap = value; }
    }
    public float[,,] terrainAlpha
    {
        get { return m_terrainTexture; }
        set { m_terrainTexture = value; }
    }
    public float propagationBias
    {
        get { return m_propagationBias; }
        set { m_propagationBias = value; }
    }
    public float propagationWindBias
    {
        get { return m_propagationWindBias; }
        set { m_propagationWindBias = value; }
    }
    public float propagationHillBias
    {
        get { return m_propagationHillBias; }
        set { m_propagationHillBias = value; }
    }
    public float maxHillPropagationDistance
    {
        get { return m_maxHillPropagationDistance; }
        set { m_maxHillPropagationDistance = value; }
    }
    public float visualExtinguishThreshold
    {
        get { return m_visualExtinguishThreshold; }
        set { m_visualExtinguishThreshold = value; }
    }

    public bool daytime
    {
        get { return m_dayTime; }
        set { m_dayTime = value; }
    }
    public bool dirty
    {
        get { return m_dirty; }
        set { m_dirty = value; }
    }
    public Terrain terrain { get { return m_terrain; } }
    public float combustionRate { get { return m_combustionRate; } }
    public float nodeCombustionRate { get { return m_nodeCombustionRate; } }
    public float terrainDetailSize { get { return m_terrainDetailSize; } }
    public int terrainWidth { get { return m_terrainDetailWidth; } }
    public int terrainHeight { get { return m_terrainDetailHeight; } }
    public int alphaWidth { get { return m_terrainAlphaWidth; } }
    public int alphaHeight { get { return m_terrainAlphaHeight; } }
    public FireTerrainTexture[] terrainTextures { get { return m_terrainTextures; } }
    public bool detailedSimulation { get { return m_detailedSimulation; } }
    public Vector2[] cellFireSpawnPositions { get { return m_cellFireSpawnPositions; } }
    public bool staggeredGridConstruction { get { return m_staggeredGridConstruction; } }
    public bool removeGrassOnceBurnt { get { return m_removeGrassOnceBurnt; } }
    public bool maxGrassDetails { get { return m_maxGrassDetails; } }
    public int burntGrassDetailIndex { get { return m_burntGrassDetailIndex; } }
    public int burntGrassTextureIndex { get { return m_burntGrassTextureIndex; } }

    void Awake()
    {
        // Cannot be smaller then cell size otherwise fire will not propagate
        if (m_maxHillPropagationDistance < m_cellSize)
            m_maxHillPropagationDistance = m_cellSize;

        // Make sure within 0->1 range 
        if (m_visualExtinguishThreshold > 1.0f)
            m_visualExtinguishThreshold = 1.0f;
        if (m_visualExtinguishThreshold < 0.0f)
            m_visualExtinguishThreshold = 0.0f;

        if (m_combustionRate < 1.0f)
            m_combustionRate = 1.0f;

        if (m_propagationBias < 0.0000001f)
        {
            m_propagationBias = 0.0000001f;
            Debug.Log("Capping propagationBias to 0.0000001f, as it's to smaller or zero");
        }

        if (m_propagationBias > 1.0f)
        {
            m_propagationBias = 1.0f;
            Debug.Log("Capping propagationBias to 1.0f, as it's too large");
        }

        if (m_propagationHillBias < 1.0f)
        {
            m_propagationHillBias = 1.0f;
            Debug.Log("Capping propagationHillBias to 1.0f, as it's too small");
        }

        // Get the terrain, need to be a child of a Terrain GameObject
        m_terrain = GetComponentInParent<Terrain>();

        if (m_terrain != null)
        {
            m_terrainDetailWidth = m_terrain.terrainData.detailWidth;
            m_terrainDetailHeight = m_terrain.terrainData.detailHeight;
            m_terrainAlphaWidth = m_terrain.terrainData.alphamapWidth;
            m_terrainAlphaHeight = m_terrain.terrainData.alphamapHeight;

            // Use the arrays or the lists
            if (!m_maxGrassDetails)
            {
                m_terrainMap = m_terrain.terrainData.GetDetailLayer(0, 0, m_terrainDetailWidth, m_terrainDetailHeight, 0);
                m_terrainReplaceMap = m_terrain.terrainData.GetDetailLayer(0, 0, m_terrainDetailWidth, m_terrainDetailHeight, 1);
                m_terrainMapOriginal = (int[,])m_terrainMap.Clone(); // performs a deep copy, Clone by itself performs a shallow copy (i.e. 2nd array has references to the 1st array)
                m_terrainReplaceMapOriginal = (int[,])m_terrainReplaceMap.Clone();
            }
            else
            {
                // Make sure a valid index was set
                if (m_burntGrassDetailIndex >= m_terrain.terrainData.detailPrototypes.Length || m_burntGrassDetailIndex < 0)
                {
                    m_burntGrassDetailIndex = 0;
                    Debug.Log("Burnt Grass Texture Index is higher/lower then the number of grass texture details set, setting to 0");
                }

                // Set up Lists
                m_terrainMaps = new List<int[,]>();
                m_terrainMapsOriginal = new List<int[,]>();
                for (int i = 0; i < m_terrain.terrainData.detailPrototypes.Length; i++)
                {
                    m_terrainMaps.Add(m_terrain.terrainData.GetDetailLayer(0, 0, m_terrainDetailWidth, m_terrainDetailHeight, i));
                    m_terrainMapsOriginal.Add(m_terrain.terrainData.GetDetailLayer(0, 0, m_terrainDetailWidth, m_terrainDetailHeight, i));
                }
            }

            // Get the terrain textures
            m_terrainTexture = m_terrain.terrainData.GetAlphamaps(0, 0, m_terrainAlphaWidth, m_terrainAlphaHeight);
            m_terrainTextureOriginal = (float[,,])m_terrainTexture.Clone();


            int TerrainDetailMapSize = m_terrain.terrainData.detailResolution;
            if (m_terrain.terrainData.size.x != m_terrain.terrainData.size.z)
            {
                Debug.Log("X and Y size of terrain have to be the same.");
                return;
            }

            // Need to have at least one terrain texture defined
            if (terrainTextures.Length != terrainAlpha.GetLength(2))
            {
                Debug.LogError("A different number of Terrain Textures are set in Fire Manager compared with the Terrain.");
            }

            m_terrainDetailSize = TerrainDetailMapSize / m_terrain.terrainData.size.x;

            if (m_cellFireSpawnPositions.Length == 0)
                m_cellFireSpawnPositions = new Vector2[1] { new Vector2(0.5f, 0.5f) };
        }
        else
        {
            Debug.LogError("Terrain not found! A Fire Manager should be a child of a Terrain GameObject.");
        }
    }

    // Use this for initialization
    void Start () {
    }
	
	// Update is called once per frame
	void Update ()
    {
        // Make sure there is a FireGrid somewhere in the world
        if (m_activeFireGrids > 0)
        {
            // Start now, so when the first fire goes out the terrain will be instantly updated
            m_terrainUpdateTimer += Time.deltaTime;

            // If dirty, update the terrain. This is set when terrain data is changed like grass being removed by FireGrassRemover
            // also make sure one of the features has been turned one
            if (m_dirty)
            {
                // Make sure the set amount of time has past, then call coroutine
                if (m_terrainUpdateTimer >= m_terrainUpdateTime)
                {
                    StartCoroutine(CoTerrainUpdate());
                    m_terrainUpdateTimer = 0.0f;
                }
            }
        }
	}

    // brief Increases the active fire gird count
    public void AddActiveFireGrid()
    {
        m_activeFireGrids++;
    }

    // brief Decreases the active fire gird count
    public void RemoveActiveFireGrid()
    {
        m_activeFireGrids--;
    }

    // brief When the application to closed, needs to reset the terrain data to the original data
    // Mostly for editor use
    void OnApplicationQuit()
    {
        if (m_terrain != null)
        {
            if (!m_maxGrassDetails)
            {
                m_terrain.terrainData.SetDetailLayer(0, 0, 0, m_terrainMapOriginal);
                m_terrain.terrainData.SetDetailLayer(0, 0, 1, m_terrainReplaceMapOriginal);    
            }
            else
            {
                for (int i = 0; i < m_terrain.terrainData.detailPrototypes.Length; i++)
                    m_terrain.terrainData.SetDetailLayer(0, 0, i, m_terrainMapsOriginal[i]);
            }

            m_terrain.terrainData.SetAlphamaps(0, 0, m_terrainTextureOriginal);
            Debug.Log("Restoring map original data");
        }
    }

    // brief Updates the terrain data (textures and details)
    // return IEnumerator as is a CoRoutine
    public IEnumerator CoTerrainUpdate()
    {
        // Using fixed array of 2 or a dynamic number of lists to store the terrain layers
        if (!m_maxGrassDetails)
        {
            // Remove or replace grass
            if (m_removeGrassOnceBurnt)
            {
                // Remove grass
                m_terrain.terrainData.SetDetailLayer(0, 0, 0, terrainMap);
                yield return null;
            }
            else
            {
                // Replace grass
                m_terrain.terrainData.SetDetailLayer(0, 0, 0, m_terrainMap);
                yield return null;
                m_terrain.terrainData.SetDetailLayer(0, 0, 1, m_terrainReplaceMap);
                yield return null;
            }

            // Replacing the terrain texture with the scorch mark texture?
            if (m_replaceGroundTextureOnceBurnt)
            {
                m_terrain.terrainData.SetAlphamaps(0, 0, m_terrainTexture);
                yield return null;
            }
        }
        else
        {
            for (int i = 0; i < m_terrain.terrainData.detailPrototypes.Length; i++)
            {
                if (m_removeGrassOnceBurnt)
                {
                    // Remove grass
                    m_terrain.terrainData.SetDetailLayer(0, 0, i, m_terrainMaps[i]);
                    yield return null;
                }
                else
                {
                    // Replace grass
                    m_terrain.terrainData.SetDetailLayer(0, 0, i, m_terrainMaps[i]);
                    yield return null;
                    m_terrain.terrainData.SetDetailLayer(0, 0, m_burntGrassDetailIndex, m_terrainMaps[m_burntGrassDetailIndex]);
                    yield return null;
                }
            }

            // Replacing the terrain texture with the scorch mark texture?
            if (m_replaceGroundTextureOnceBurnt)
            {
                m_terrain.terrainData.SetAlphamaps(0, 0, m_terrainTexture);
                yield return null;
            }
        }

        // Reset dirty flag
        m_dirty = false;
    }
}
