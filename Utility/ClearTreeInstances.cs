using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utility
{
	public class ClearTreeInstances: MonoBehaviour
	{
		[SerializeField] private Terrain terrain;

		[Button("Clear tree instances")]
		public void Clear()
		{
			terrain.terrainData.treeInstances = Array.Empty<TreeInstance>();
		}
	}
}