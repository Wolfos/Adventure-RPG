using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = Unity.Mathematics.Random;

namespace Utility
{
	[Serializable]
	public class TreeDefinition
	{
		public GameObject prefab;
		[Range(0.1f, 3f)] public float minSize = 1;
		[Range(0.1f, 3f)] public float maxSize = 1.5f;
		public int spawnChance = 50;
		[HideInInspector] public int prototypeId;
	}
	
	[RequireComponent(typeof(SplineContainer)), ExecuteInEditMode]
	public class SplineTreeSpawner: MonoBehaviour
	{
		[SerializeField] private TreeDefinition[] trees;
		
		[SerializeField] private float resolution = 1;
		[SerializeField] private uint randomSeed = 1;
		[SerializeField] private float noiseScale = 1;
		[SerializeField] private float noiseCutoff = 0.5f;
		[SerializeField] private float noiseCutoffRandom = 0.1f;
		[SerializeField] private float noiseOffset;
		[Range(0, 1), SerializeField] private float maxDensity = 1;
		[SerializeField] private Terrain[] terrains;
		[SerializeField] private byte uniqueIdentifier;
		[SerializeField] private SplineContainer[] excluders;
		[SerializeField] private SplineContainer[] roads;
		[SerializeField] private float maxDistanceFromRoad = 3;

		private int _treesSpawned;

		private List<Vector3> _excluded = new();

		[Button("Add trees to terrains")]
		public void AddTrees()
		{
			foreach (var terrain in terrains)
			{
				var terrainData = terrain.terrainData;
				var prototypes = terrainData.treePrototypes.ToList();
				
				foreach (var tree in trees)
				{
					// If terrain doesn't have this tree
					if (terrainData.treePrototypes.Count(p => p.prefab == tree.prefab) == 0)
					{
						var prototype = new TreePrototype
						{
							prefab = tree.prefab
						};
						prototypes.Add(prototype);
					}
				}

				terrainData.treePrototypes = prototypes.ToArray();
			}
		}

		[Button("Update")]
		public void UpdateTrees()
		{
			ClearTrees();
			_excluded.Clear();
			_treesSpawned = 0;
			
			var rng = new Random(randomSeed);
			var splineContainer = GetComponent<SplineContainer>();
			foreach (var spline in splineContainer.Splines)
			{
				var bounds = spline.GetBounds();
				float3 position = transform.position;
				
				for (var x = bounds.min.x; x < bounds.max.x; x += resolution)
				{
					for (var z = bounds.min.z; z < bounds.max.z; z += resolution)
					{
						var pos = new float3(x, 0, z);
						pos.x += rng.NextFloat(-resolution / 2, resolution / 2);
						pos.z += rng.NextFloat(-resolution / 2, resolution / 2);
						var perlin = Mathf.PerlinNoise(pos.x * noiseScale + noiseOffset, pos.z * noiseScale + noiseOffset);
						perlin = math.clamp(perlin, 1 - maxDensity, 1);

						if(perlin > noiseCutoff + rng.NextFloat() * noiseCutoffRandom) continue;
						
						if (IsInsideSpline(pos, spline))
						{
							var worldPosition = pos + position;
							
							var terrain = GetTerrainAtPosition(worldPosition);
							if (terrain == null) continue;

							var samplePos = (Vector3) worldPosition;
							var height = terrain.SampleHeight(samplePos);
							worldPosition.y = height;
							
							foreach (var excluder in excluders)
							{
								foreach (var s in excluder.Splines)
								{
									if (IsInsideSpline(worldPosition - (float3)excluder.transform.position, s))
									{
										_excluded.Add(worldPosition);
										goto Excluded;
									}
								}
							}

							foreach (var road in roads)
							{
								foreach (var s in road.Splines)
								{
									var distance = SplineUtility.GetNearestPoint(s, worldPosition - (float3) road.transform.position,
										out _, out _);
									if (distance < maxDistanceFromRoad)
									{
										_excluded.Add(worldPosition);
										goto Excluded;
									}
								}
							}
							
							AddTree(terrain, worldPosition, rng);
							//terrain.terrainData.treeInstances = Array.Empty<TreeInstance>();
							
							//Debug.Log(terrain.terrainData.treeInstanceCount);
							Excluded: ;
						}
					}
				}
			}
			
			Debug.Log($"Spawned {_treesSpawned} trees");
		}

		[Button("Clear trees")]
		private void ClearTrees()
		{
			foreach (var terrain in terrains)
			{
				var newTreeInstances = new List<TreeInstance>(terrain.terrainData.treeInstances);
				
				for(var i = terrain.terrainData.treeInstanceCount - 1; i >= 0; i--)
				{
					var tree = terrain.terrainData.treeInstances[i];
					if (tree.lightmapColor.r == uniqueIdentifier)
					{
						newTreeInstances.RemoveAt(i);
					}
				}
				
				terrain.terrainData.SetTreeInstances(newTreeInstances.ToArray(), false);
			}
		}

		private void AddTree(Terrain terrain, Vector3 position, Random rng)
		{
			var terrainData = terrain.terrainData;
			
			// Normalize position, as it expects a value between 0 and 1
			position -= terrain.transform.position;
			position.x /= terrainData.size.x;
			position.z /= terrainData.size.z;
			position.y = 0;
			
			var prototype = SelectTreePrototype(terrainData, rng);
			if (prototype == null) return;
			
			var size = rng.NextFloat(prototype.minSize, prototype.maxSize);
			var treeInstance = new TreeInstance
			{
				prototypeIndex = prototype.prototypeId,
				position = position,
				widthScale = size,
				heightScale = size,
				lightmapColor = new (uniqueIdentifier, 0, 0, 0) // This isn't actually displayed in-game, so it's used to identify the trees placed by the spline tool
			};
			
			terrain.AddTreeInstance(treeInstance);
			_treesSpawned++;
		}

		private TreeDefinition SelectTreePrototype(TerrainData terrainData, Random rng)
		{
			float totalChance = 0;
			foreach (var tree in trees)
			{
				totalChance += tree.spawnChance;
				for (int i = 0; i < terrainData.treePrototypes.Length; i++)
				{
					var prototype = terrainData.treePrototypes[i];
					
					if (prototype.prefab == tree.prefab)
					{
						tree.prototypeId = i;
						goto Found;
					}
				}
				
				Debug.LogError("Tree prototype was not on terrain!");
				return null;
				Found: ;
			}

			var random = rng.NextFloat(0, totalChance);
			float chance = 0;
			foreach (var tree in trees)
			{
				chance += tree.spawnChance;
				if (chance >= random) return tree;
			}

			return null;

		}

		private Terrain GetTerrainAtPosition(float3 position)
		{
			foreach (var terrain in terrains)
			{
				var terrainPos = terrain.transform.position;
				var size = terrain.terrainData.size;
				if (position.x > terrainPos.x && position.x < terrainPos.x + size.x &&
				    position.z > terrainPos.z && position.z < terrainPos.z + size.z)
				{
					return terrain;
				}
			}

			return null;
		}
		

		/// <summary>
		/// Whether a point is inside a closed spline.
		/// </summary>
		/// <param name="point">A point, local to the spline's space</param>
		/// <param name="spline">Unity spline</param>
		/// <returns>Whether the point is inside the spline</returns>
		private bool IsInsideSpline(float3 point, Spline spline)
		{
			var bounds = spline.GetBounds();
			// Is outside of bounds?
			if (point.x < bounds.min.x || point.x > bounds.max.x ||
			    point.z < bounds.min.z || point.z > bounds.max.z)
			{
				return false;
			}
			SplineUtility.GetNearestPoint(spline, point, out var splinePoint, out var t);
			spline.Evaluate(t, out _, out var tangent, out _);
			
			var cross = math.cross(math.up(), math.normalize(tangent));
			return math.dot(splinePoint - point, cross) < 0;
		}

		private void OnDrawGizmosSelected()
		{
			Gizmos.color = Color.red;
			foreach (var pos in _excluded)
			{
				Gizmos.DrawSphere(pos, 1);
			}
		}
	}
}