using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace World
{
    public class GrassSpawner : MonoBehaviour
    {
        [Serializable]
        private struct SpawnRule
        {
            public int detailPrototype;
            [Range(0, 1)] public float coverage;
            [Range(0, 5)] public int strength;
            [FormerlySerializedAs("layer")] public int splatLayer;
            [Range(0, 1)] public float threshold;
        }

        [SerializeField] private SpawnRule[] spawnRules;
        
        [Button("Generate")]
        public void GenerateGrass()
        {
            var terrainData = GetComponent<Terrain>().terrainData;
			var detailResolution = terrainData.detailResolution;
			
			var splatMaps = terrainData.GetAlphamaps(0,0,terrainData.alphamapResolution,terrainData.alphamapResolution);
			var resolutionMultiplier = (1.0f / detailResolution)*terrainData.alphamapResolution;

			foreach (var spawnRule in spawnRules)
			{
				var layerData = new int[detailResolution, detailResolution];
				
				for(var y = 0; y < detailResolution; y++)
				{
					for(var x = 0; x < detailResolution; x++)
					{
						if (Random.value > spawnRule.coverage) continue;
						
						var splatAmount = splatMaps[(int)(resolutionMultiplier*x),(int)(resolutionMultiplier*y),spawnRule.splatLayer];
						if (splatAmount < spawnRule.threshold) continue;
						
						layerData[x, y] = spawnRule.strength;
					}
				}
				
				terrainData.SetDetailLayer(0, 0, spawnRule.detailPrototype, layerData);
			}
        }
    }
}