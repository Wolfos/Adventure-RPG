using System;
using System.Collections.Generic;
using UnityEngine;

namespace FX
{
    [ExecuteAlways]
    public class GrassRenderer : MonoBehaviour
    {
        [SerializeField] private Terrain terrain;
        [SerializeField] private Mesh grassMesh;
        [SerializeField] private Material grassMaterial;
        [SerializeField] private int radius;

        public struct InstanceData
        {
            public Matrix4x4 objectToWorld;
        }

        private void Update()
        {
            var cameraPosition = new Vector3();
            #if UNITY_EDITOR
            cameraPosition = UnityEditor.SceneView.lastActiveSceneView.camera.transform.position;
            #endif
            var terrainData = terrain.terrainData;
            var instanceData = new List<InstanceData>();
            for (int x = -radius/2; x < radius/2; x++)
            {
                for (int y = -radius/2; y < radius/2; y++)
                {
                    var position = cameraPosition + new Vector3(x, 0, y);
                    position.y = terrain.SampleHeight(position);

                    var terrainPos = GetTerrainCoordinates(position, terrain);
                    var splats = terrainData.GetAlphamaps(Mathf.FloorToInt(terrainPos.x)+radius/2, Mathf.FloorToInt(terrainPos.y)+radius/2, 1, 1);
                    if (splats[0, 0, 0] < 0.5f) continue;
                    
                    var data = new InstanceData
                    {
                        objectToWorld = Matrix4x4.Translate(position)
                    };
                    instanceData.Add(data);
                }
            }

            var rp = new RenderParams(grassMaterial);
            Graphics.RenderMeshInstanced(rp, grassMesh, 0, instanceData);
        }
        
        private Vector2 GetTerrainCoordinates(Vector3 worldPoint, Terrain terrain)
        {
            var offset = radius / 2; //This offsets the hit position to account for the size of the brush which gets drawn from the corner out
            //World Position Offset Coords, these can differ from the terrain coords if the terrain object is not at (0,0,0)
            Vector3 tempTerrainCoodinates = worldPoint - terrain.transform.position;
            //This takes the world coords and makes them relative to the terrain
            var data = terrain.terrainData;
            Vector3 terrainCoordinates = new Vector3(
                tempTerrainCoodinates.x / data.size.x,
                tempTerrainCoodinates.y / data.size.y,
                tempTerrainCoodinates.z / data.size.z);

            // This will take the coords relative to the terrain and make them relative to the height map(which often has different dimensions)
            var terrainData = data;
            Vector3 locationInTerrain = new Vector3
            (
                terrainCoordinates.x * terrainData.alphamapWidth,
                0,
                terrainCoordinates.z * terrainData.alphamapHeight
            );

            return new(locationInTerrain.x - offset, locationInTerrain.z - offset);
        }
    }
}