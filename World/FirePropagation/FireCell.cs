/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 10/01/2017
*/
using UnityEngine;
using System.Collections;

public class FireCell : MonoBehaviour {
    private GameObject m_firePrefab; // prefab to be used for the fire
    private GameObject[] m_fires; // fire
    private FireBox m_fireBox = null; // used to detect collision with GameObjects that have colliders
    [Tooltip("The Hit Points of the cell, the higher the HP to slower the cell is to heat up and ignite.")]
    public float m_HP = 50.0f;
    [Tooltip("Amount of fuel in the cell.")]
    public float m_fuel = 60.0f;
    private float m_extinguishThreshold; // the level when the extingush stage particle systems should be run
    [Tooltip("The amount of ground moisture in the cell.")]
    public float m_moisture = 0.0f;
    private float m_ignitionTemperature = 1.0f;
    private float m_fireTemperature = 0.0f;
    private float m_combustionConstant = 1.0f;
    private float m_pIgnition = 0.0f;
    private bool m_instantiatedInCell = false;
    private bool m_temperatureModified = false;
    private bool m_fireProcessHappening = false;
    private bool m_fireJustStarted = false;
    private bool m_isAlight = false;
    private bool m_extinguish = false;
    private bool m_extingushed = false;
    private int m_iginitionCounter = 0;
    private Vector2[] m_firesPositions;

    public Vector3 position { get { return transform.position; } }
    public float fireTemperature
    {
        set { m_fireTemperature = value;  }
        get { return m_fireTemperature; }
    }
    public float ignitionTemperature
    {
        set { m_ignitionTemperature = value; }
        get { return m_ignitionTemperature; }
    }
    public bool temperatureModified
    {
        set { m_temperatureModified = value; }
        get { return m_temperatureModified; }
    }
    public float extinguishThreshold { get { return m_extinguishThreshold; } }
    public bool fireInCell { get { return m_instantiatedInCell; } }
    public bool isAlight { get { return m_isAlight; } }
    public bool fireProcessHappening { get { return m_fireProcessHappening; } }

	// Update is called once per frame
	void Update ()
    {
        // If this cell is heating up but the fire moves away and can no longer heat it up, extingush this cell
        if (!m_fireJustStarted && !m_isAlight && m_fireProcessHappening && m_pIgnition == m_ignitionTemperature)
            m_iginitionCounter++;
        else
            m_iginitionCounter = 0;

        if (m_iginitionCounter > 300)
            m_extinguish = true;

        m_pIgnition = m_ignitionTemperature;
    }

    // brief Allows inital data values to be set
    // param bool If the cell is already alight
    // param GameObject The GameObject/Prefab to be used for the fire
    // param CellData The cell data
    // param String Name of the terrain
    // param Vector2[] The positions within the cell a fire should be instantiated, requires at least 1 position in the cell
    public void SetupCell(bool alight, GameObject fire, CellData data, string terrainName, Vector2[] firesPositionsInCell)
    {
        m_isAlight = alight;
        m_firePrefab = fire;
        m_fires = new GameObject[firesPositionsInCell.Length];
        m_firesPositions = firesPositionsInCell;
        m_HP = data.HP;
        m_fuel = data.fuel;
        m_extinguishThreshold = data.fuel * data.threshold;
        m_moisture = data.moisture;
        m_fireBox = new FireBox();
        m_fireBox.Init(transform.position, terrainName);
        float boxExtents = data.cellSize / 2.0f;
        m_fireBox.radius = new Vector3(boxExtents, boxExtents, boxExtents);
        m_combustionConstant = data.combustionValue;
        SetInitialFireValues(data.airTemperature, data.propagationSpeed);
    }

    // brief Delete the fire
    public void Delete()
    {
        // Destory the fire in the cell now or after a delay
        if (m_instantiatedInCell && m_extingushed)
        {
            gameObject.SetActive(false);
            m_fireProcessHappening = false;
        }
        else if (m_instantiatedInCell)
        {
            gameObject.SetActive(false);
            m_fireProcessHappening = false;
        }
    }

    // brief Update that is called by a FireGrid
    // param FireGrassRemover A script that can remove grass from the terrain
    public void GridUpdate(FireGrassRemover script)
    {
        // Combustion will not start until ignition() is called in heatsUp() - called by FireGrid
        // this will ensure that no fire is instantiated and is deleted at the right time
        Combustion();

        if (m_extinguish && !m_extingushed)
        {
            // delete fire particle system
            script.DeleteGrassOnPosition(transform.position);
            m_extingushed = true;

            if (m_fireBox != null)
            {
                m_fireBox = null;
            }

            Delete();
        }
    }

    // brief Instantiate a fire at position
    // param Vector3 Position
    // param GameObject Fire GameObject/Prefab
    private void InstantiateFire(Vector3 position, GameObject Fire)
    {
        for (int i = 0; i < m_fires.Length; i++)
        {
            m_fires[i] = (GameObject)Instantiate(Fire, position + new Vector3(m_firesPositions[i].x, 0.0f, m_firesPositions[i].y), new Quaternion(), transform);
        }
    }

    // brief The ignition step, if a fire has just started in the cell set internal and Fire Visual Manager states. Ignition() should be 
    // called rather then this method directly
    // param Vector3 Position
    // param GameObject Fire GameObject/Prefab
    public void Ignition(Vector3 position, GameObject Fire)
    {
        if (m_fireJustStarted)
        {
            m_isAlight = true;

            for (int i = 0; i < m_fires.Length; i++)
            {
                FireVisualManager visualMgr = m_fires[i].GetComponent<FireVisualManager>();
                visualMgr.SetIgnitionState();
            }
        }
    }

    // brief Computes what ignition and fire temperatures should be depending on different factors that affect the behaviour of the fire propagation
    // param float Air temperature 
    // param float Global speed of fire propagation
    public void SetInitialFireValues(float airTemp, float globalFirePropagationSpeed)
    {
        m_ignitionTemperature = (m_HP - airTemp) + m_moisture;

        if (m_HP > 0.0f)
            m_fireTemperature += (m_fuel / m_HP) + globalFirePropagationSpeed;
        else
            m_fireTemperature += globalFirePropagationSpeed;
    }

    // brief The heats up step, removing the cell's hit points until the ignition temperature is met
    public void HeatsUp()
    {
        if (m_instantiatedInCell == false)
        {
            InstantiateFire(transform.position, m_firePrefab);
            m_instantiatedInCell = true;
            m_fireProcessHappening = true;

            for (int i = 0; i < m_fires.Length; i++)
            {
                FireVisualManager visualMgr = m_fires[i].GetComponent<FireVisualManager>();
                visualMgr.SetHeatState();
            }
        }

        if (m_ignitionTemperature > 0.0f)
            m_ignitionTemperature -= m_fireTemperature * Time.deltaTime;

        if (m_ignitionTemperature <= 0.0f && !m_isAlight)
        {
            m_fireJustStarted = true;
            Ignition();
        }
    }

    // brief Call the ignition step depending on internal states
    void Ignition()
    {
        if (m_fireJustStarted && !m_extingushed)
            Ignition(transform.position, m_firePrefab);
    }

    // brief The combustion step, if the fire is alight consume fuel in the cell until the fuel has run out
    void Combustion()
    {
        if (m_isAlight)
        {
            m_fireJustStarted = false;

            // Use fuel
            m_fuel -= m_combustionConstant * Time.deltaTime;

            // Check if threshold has been met, if so set Fire Visual Manager state
            if(m_fuel < m_extinguishThreshold)
            {
                for (int i = 0; i < m_fires.Length; i++)
                {
                    FireVisualManager visualMgr = m_fires[i].GetComponent<FireVisualManager>();
                    visualMgr.SetExtingushState();
                }
            }

            // Run out of fuel? If so set internal states
            if (m_fuel <= 0.0f)
            {
                m_isAlight = false;
                m_extinguish = true;
            }

            // is there a collision in this cell with a GameObject with a FireNodeChain
            m_fireBox.DetectionTest();
        }
    }
}
