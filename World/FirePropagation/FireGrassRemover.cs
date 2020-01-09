/* Copyright (c) 2016-2017 Lewis Ward
// Fire Propagation System
// author: Lewis Ward
// date  : 16/01/2017
*/
using UnityEngine;
using System.Collections;

public class FireGrassRemover : MonoBehaviour {
    private FireManager m_fireManager;
    private float[] m_pixelPoints = new float[4];
    private float m_radius = 1;
    private int m_scorchmarkTexture = 0;
    private int m_DToAWidth = 0;
    private int m_DToAHeight = 0;
    private bool m_replaceGrass = false;
    public float radius {  set { m_radius = value; } }

    // Use this for initialization
    void Start () {
        // get the terrain from the fire manager
        m_fireManager = FindObjectOfType<FireManager>();

        if (m_fireManager != null)
        {
            // get ratio
            m_DToAWidth = m_fireManager.terrainWidth / m_fireManager.alphaWidth;
            m_DToAHeight = m_fireManager.terrainHeight / m_fireManager.alphaHeight;

            // find which texture is the scorch mark texture
            foreach (FireTerrainTexture texture in m_fireManager.terrainTextures)
            {
                if (texture.m_isGroundBurnTexture == true)
                {
                    m_scorchmarkTexture = texture.m_textureID;
                    break;
                }
            }

            m_replaceGrass = !m_fireManager.removeGrassOnceBurnt;
        }
    }

    // brief Delete any grass at defined position
    // param Vector3 Terrain position
    public void DeleteGrassOnPosition(Vector3 position)
    {
        RemoveGrass(m_fireManager.terrain, position);
    }

    // brief Delete any grass at defined position on a terrain
    // param Terrain
    // param Vector3 Terrain position to remove grass from
    void RemoveGrass(Terrain terrian, Vector3 position)
    {
        // convert the position to a position on the terrain map
        Vector3 texturePoint3D = position;
        texturePoint3D = texturePoint3D * m_fireManager.terrainDetailSize;
        m_pixelPoints[0] = texturePoint3D.z + m_radius;
        m_pixelPoints[1] = texturePoint3D.z - m_radius;
        m_pixelPoints[2] = texturePoint3D.x + m_radius;
        m_pixelPoints[3] = texturePoint3D.x - m_radius;


        // keep within array bounds
        for (int i = 0; i < 4; i++)
        {
            if (m_pixelPoints[i] < 0)
                m_pixelPoints[i] = 0;

            if (m_pixelPoints[i] > m_fireManager.terrainHeight || m_pixelPoints[i] > m_fireManager.terrainWidth)
                m_pixelPoints[i] = m_fireManager.terrainHeight - 1; // Height and Width should always be the same, checked on creation of the grid.

        }

        // Remove the grass from the terrain
        for (int y = (int)m_pixelPoints[3]; y < (int)m_pixelPoints[2] + 1; y++)
        {
            for (int x = (int)m_pixelPoints[1]; x < (int)m_pixelPoints[0] + 1; x++)
            {
                // Using the standard number of grass detail or the maximum number
                if (!m_fireManager.maxGrassDetails)
                {
                    if (m_replaceGrass && m_fireManager.terrainMap[x, y] != 0)
                    {
                        m_fireManager.terrainMap[x, y] = 0;
                        m_fireManager.terrainReplacementMap[x, y] = 1;
                    }
                    else
                    {
                        m_fireManager.terrainMap[x, y] = 0;
                    }
                }
                else
                {
                    for (int i = 0; i < m_fireManager.terrain.terrainData.detailPrototypes.Length; i++)
                    {
                        if (m_replaceGrass && m_fireManager.terrainMaps[i][x, y] != 0)
                        {
                            m_fireManager.terrainMaps[i][x, y] = 0;
                            m_fireManager.terrainMaps[m_fireManager.burntGrassDetailIndex][x, y] = 1;
                        }
                        else if(!m_replaceGrass)
                        {
                            m_fireManager.terrainMaps[i][x, y] = 0;
                        }
                    }
                }
                
                // Set the dirty flag, will trigger a terrain update
                m_fireManager.dirty = true;
            }
        }
        
        // Update the alphamap to the scorch mark texture
        int terrainLayerLen = m_fireManager.terrainAlpha.GetLength(2);
        for (int y = (int)m_pixelPoints[3]; y < (int)m_pixelPoints[2] + 1; y++)
        {
            for (int x = (int)m_pixelPoints[1]; x < (int)m_pixelPoints[0] + 1; x++)
            {
                if (m_pixelPoints[0] > x && m_pixelPoints[1] < x && m_pixelPoints[2] > y && m_pixelPoints[3] < y)
                {
                    int X = x / m_DToAWidth;
                    int Y = y / m_DToAHeight;
                
                    for (int i = 0; i < terrainLayerLen; i++)
                        m_fireManager.terrainAlpha[X, Y, i] = 0;
                
                    m_fireManager.terrainAlpha[X, Y, m_scorchmarkTexture] = 1;
                }
            }
        }
    }
}
