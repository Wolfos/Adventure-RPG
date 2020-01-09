/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 10/02/2017
*/
using UnityEngine;
using System.Collections;

public class FireIgniter : MonoBehaviour {
    [SerializeField][Tooltip("Width of the fire grid, fire starts in the center of the grid")]
    private int m_gridWidth = 10;
    [SerializeField][Tooltip("Height of the fire grid, fire starts in the center of the grid")]
    private int m_gridHeight = 10;
    [SerializeField][Tooltip("Prefab of the fire to use")]
    private GameObject m_firePrefab;
    [SerializeField][Tooltip("Delete this GameObject when there is a collision with it and the terrain or another GameObject?")]
    private bool m_destroyOnCollision = false;
    private bool m_fireIgnited = false;

    // Use this for initialization
    void Start () {
        if(m_firePrefab == null)
        {
            Debug.LogError("No Fire Prefab set on Fire Igniter.");
        }

        // negate negative values
        if (m_gridWidth < 0)
            m_gridWidth = -m_gridWidth;
        if (m_gridHeight < 0)
            m_gridHeight = -m_gridHeight;

        // valid size grid
        if (m_gridWidth == 0)
            m_gridWidth = 1;
        if (m_gridHeight == 0)
            m_gridHeight = 1;
    }

    // brief Call this once a GameObject has detected a collision
    public void OnCollision()
    {
        GameObject fireGrid = new GameObject();
        fireGrid.name = "FireGrid";
        FireGrid grid = fireGrid.AddComponent<FireGrid>();
        fireGrid.AddComponent<FireGrassRemover>();
        grid.IgniterUpdate(m_firePrefab, gameObject.transform.position, m_gridWidth, m_gridHeight);
    }

    // brief On collision
    // param Collision
    void OnCollisionEnter(Collision collision)
    {
        if (m_fireIgnited == false)
        {
            OnCollision();
            m_fireIgnited = true;

            if (m_destroyOnCollision)
                Destroy();
        }
    }

    // brief Destroy this object
    void Destroy()
    {
        Destroy(gameObject);
    }
}
