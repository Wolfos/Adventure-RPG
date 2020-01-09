/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 04/04/2017
*/
using UnityEngine;
using System.Collections;

public class FireVisualManager : MonoBehaviour {
    private ParticleSystem[] m_particleSystems;
    [Tooltip("Should be the same number as particle systems used in the Fire, which of those particle systems should be active in the simulation's heat up step.")]
    public bool[] m_heatUp;
    [Tooltip("Should be the same number as particle systems used in the Fire, which of those particle systems should be active in the simulation's ignition step.")]
    public bool[] m_ignition;
    [Tooltip("Should be the same number as particle systems used in the Fire, which of those particle systems should be active in the simulation's extingush step.")]
    public bool[] m_extinguish;
    private bool m_heatState = false;
    private bool m_ignitionState = false;
    private bool m_extinguishState = false;
    private bool m_heatStateSet = false;
    private bool m_ignitionStateSet = false;
    private bool m_extinguishStateSet = false;

    // Use this for initialization
    void Start () {

        m_particleSystems = GetComponentsInChildren<ParticleSystem>();

        if (m_heatUp.Length > m_particleSystems.Length)
            Debug.LogError(gameObject.name + " FireVisualManager::heatUp bigger then the number of children with Particle Systems");

        if (m_ignition.Length > m_particleSystems.Length)
            Debug.LogError(gameObject.name + " FireVisualManager::ignition bigger then the number of children with Particle Systems");

        if (m_extinguish.Length > m_particleSystems.Length)
            Debug.LogError(gameObject.name + " FireVisualManager::extingush bigger then the number of children with Particle Systems");

        // start off by turning all particle systems off
        //for (int i = 0; i < m_particleSystems.Length; i++)
            //m_particleSystems[i].gameObject.SetActive(false);
    }
	
	// Update is called once per frame
	void Update () {
	
        if(m_heatState && m_heatStateSet == false)
        {
            for (int i = 0; i < m_particleSystems.Length; i++)
            {
                m_particleSystems[i].gameObject.SetActive(m_heatUp[i]);
            }

            m_heatStateSet = true;
        }
        else if(m_ignitionState && m_ignitionStateSet == false)
        {
            for (int i = 0; i < m_particleSystems.Length; i++)
                m_particleSystems[i].gameObject.SetActive(m_ignition[i]);

            m_ignitionStateSet = true;
        }
        else if(m_extinguishState && m_extinguishStateSet == false)
        {
            for (int i = 0; i < m_particleSystems.Length; i++)
                m_particleSystems[i].gameObject.SetActive(m_extinguish[i]);

            m_extinguishStateSet = true;
        }
	}

    // brief Set the state to the heat state
    public void SetHeatState()
    {
        m_heatState = true;
        m_ignitionState = false;
        m_extinguishState = false;
    }

    // brief Set the state to the ignition state
    public void SetIgnitionState()
    {
        m_heatState = false;
        m_ignitionState = true;
        m_extinguishState = false;
    }

    // brief Set the state to the extingush state
    public void SetExtingushState()
    {
        m_heatState = false;
        m_ignitionState = false;
        m_extinguishState = true;
    }
}
