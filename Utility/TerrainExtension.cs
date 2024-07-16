using System;
using Player;
using UnityEngine;

namespace Utility
{
	public class TerrainExtension: MonoBehaviour
	{
		[HideInInspector]
		public string guid;

		[SerializeField] private float unloadDistance = 3000;

		private Vector3 _position;
		private Terrain _terrain;

		private void Awake()
		{
			// _position = transform.position; // A terrain is assumed to never move
			// _terrain = GetComponent<Terrain>();
		}

		public void Initialize()
		{
			guid = Guid.NewGuid().ToString();
		}

		private void Update()
		{
			// var cameraPosition = PlayerCamera.GetCameraPosition();
			// var squareDistance = (_position - cameraPosition).sqrMagnitude;
			// var shouldBeActive = squareDistance < unloadDistance * unloadDistance;
			//
			// _terrain.enabled = shouldBeActive;
		}
	}
}