using System;
using UnityEngine;

namespace Utility
{
	public class TerrainExtension: MonoBehaviour
	{
		[HideInInspector]
		public string guid;

		public void Initialize()
		{
			guid = Guid.NewGuid().ToString();
		}
	}
}