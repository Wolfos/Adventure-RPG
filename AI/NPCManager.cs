using System;
using System.Collections.Generic;
using Character;
using UnityEngine;

namespace AI
{
	public class NPCManager: MonoBehaviour
	{
		private static List<NPC> _allNPCs = new();
		private static List<NPC> _activeNPCs = new();
		private static List<NPC> _inactiveNPCs = new();

		[SerializeField] private float cullingDistance = 100;

		private Transform _cameraTransform;

		public static void Register(NPC npc)
		{
			_allNPCs.Add(npc);
			_activeNPCs.Add(npc);
		}

		private void Awake()
		{
			_cameraTransform = Camera.main.transform;
		}


		private void Update()
		{
			DistanceCulling();
		}

		private void OnDestroy()
		{
			_allNPCs.Clear();
			_activeNPCs.Clear();
			_inactiveNPCs.Clear();
		}

		private void DistanceCulling()
		{
			var cameraPosition = _cameraTransform.position;
			for (int i = _inactiveNPCs.Count - 1; i >= 0; i--)
			{
				var npc = _inactiveNPCs[i];
				var squareDistance = Vector3.SqrMagnitude(cameraPosition - npc.CharacterComponent.Position);
				if (squareDistance < cullingDistance * cullingDistance)
				{
					npc.gameObject.SetActive(true);
					npc.Resume();
					
					_activeNPCs.Add(npc);
					_inactiveNPCs.RemoveAt(i);
				}
			}

			for (int i = _activeNPCs.Count - 1; i >= 0; i--)
			{
				var npc = _activeNPCs[i];
				var squareDistance = Vector3.SqrMagnitude(cameraPosition - npc.CharacterComponent.Position);
				if (squareDistance > cullingDistance * cullingDistance)
				{
					npc.gameObject.SetActive(false);
					npc.StopAllCoroutines();
					
					_inactiveNPCs.Add(npc);
					_activeNPCs.RemoveAt(i);
				}
			}
		}
	}
}