/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 01/02/2017
*/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FireNode : MonoBehaviour
{
    [Tooltip("Prefab to be used for the fire.")]
    public GameObject m_fire;
    [Tooltip("GameObjects with a FireNode script, any linked node will be heated up once this node is on fire.")]
    public List<GameObject> m_links = null;
    [Tooltip("The Hit Points of the cell, the higher the HP to slower the cell is to heat up and ignite.")]
    public float m_HP = 50.0f;
    [Tooltip("Amount of fuel in the cell.")]
    public float m_fuel = 50.0f;
    private float m_extinguishThreshold;
    private float m_combustionRateValue;
    private bool m_fireJustStarted = false;
    private bool m_isAlight = false;
    private bool m_extingushed = false;
    private bool m_clean = false;
    private FireVisualManager m_visualMgr = null;
    public GameObject flames { get { return m_fire; } }
    public bool isAlight { get { return m_isAlight; } }
    public bool fireJustStarted
    {
        get { return m_fireJustStarted; }
        set { m_fireJustStarted = value; }
    }
    public float HP
    {
        get { return m_HP; }
        set { m_HP = value; }
    }
    public float extinguishThreshold { get { return m_extinguishThreshold; } }


    // Use this for initialization
    void Start()
    {
        // If a tag was not set in the editor then fallback to slower why of finding the object
        try
        {
            GameObject manager = GameObject.FindWithTag("Fire");

            if (manager != null)
            {
                FireManager fireManager = manager.GetComponent<FireManager>();

                if (fireManager != null)
                {
                    m_extinguishThreshold = m_fuel * fireManager.visualExtinguishThreshold;
                    m_combustionRateValue = fireManager.nodeCombustionRate;
                }
                else
                {
                    m_extinguishThreshold = m_fuel;
                    m_combustionRateValue = 1.0f;
                }
            }
        }
        catch
        {
            // Get the terrain from the fire manager
            FireManager fireManager = FindObjectOfType<FireManager>();

            if (fireManager != null)
            {
                m_extinguishThreshold = m_fuel * fireManager.visualExtinguishThreshold;
                m_combustionRateValue = fireManager.nodeCombustionRate;
            }
            else
            {
                m_extinguishThreshold = m_fuel;
                m_combustionRateValue = 1.0f;
            }
        }
    }

    // brief Kills the attached child particle systems
    private void KillFlames()
    {
        Destroy(m_fire);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_HP <= 0.0f && !m_isAlight)
            m_fireJustStarted = true;

        Ingition();
        Combustion();
    }

    // brief Has this node ran out of fire fuel
    // return bool true if it has
    public bool NodeConsumed()
    {
        if (m_clean == true)
            return true;
        else
            return false;
    }

    // brief Force this script to update
    public void ForceUpdate()
    {
        Update();
    }

    // brief Creates a fire of the set Fire Prefab within the Fire Manager
    // param Vector3 Position to spawn fire
    // param GameObject The fire GameObject
    public void InstantiateFire(Vector3 position, GameObject Fire)
    {
        if (m_fireJustStarted)
        {
            m_fire = (GameObject)Instantiate(Fire, position, new Quaternion());

            // Should be set after fire extinguished
            m_isAlight = true;
        }
    }

    // brief The ingition step, will make the fuel within the cell become lit
    void Ingition()
    {
        if (m_fireJustStarted && !m_extingushed)
        {
            InstantiateFire(transform.position, m_fire);
            m_fireJustStarted = false;

            GetVisualManager();

            if(m_visualMgr != null)
                m_visualMgr.SetIgnitionState();
        }
    }

    // brief The combustion step, triggered after the fire is alight
    void Combustion()
    {
        if (m_isAlight)
        {
            m_fireJustStarted = false;

            m_fuel -= m_combustionRateValue * Time.deltaTime;

            if (m_fuel < m_extinguishThreshold)
            {
                // Should be valid as getFireManager() called in ingition before this function is called
                if (m_visualMgr != null)
                    m_visualMgr.SetExtingushState();
            }

            if (m_fuel <= 0.0f)
            {
                m_isAlight = false;
                m_extingushed = true;

                KillFlames();
                m_clean = true;
            }
        }
    }

    // brief Gets the visual manager and sets a reference to it
    void GetVisualManager()
    {
        if (m_visualMgr == null)
            m_visualMgr = m_fire.GetComponent<FireVisualManager>();
    }
}
