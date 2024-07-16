using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Data;
using OpenWorld;
using UnityEngine;

namespace AI
{
	public class NPCManager: SaveableObject
	{
		private class NPCSaveData : ISaveData
		{
			public List<Tuple<string, string>> Json { get; set; }
		}
		
		private static List<NPC> _allNPCs = new();
		private static List<NPC> _activeNPCs = new();
		private static List<NPC> _inactiveNPCs = new();

		[SerializeField] private float cullingDistance = 100;

		private Transform _cameraTransform;
		private NPCSaveData _saveData = new();

		public static void Register(NPC npc)
		{
			_allNPCs.Add(npc);
			_activeNPCs.Add(npc);
		}

		private void Awake()
		{
			_cameraTransform = Camera.main.transform;
		}

		private void Start()
		{
			if (SaveGameManager.HasData(id))
			{
				_saveData = SaveGameManager.GetData(id) as NPCSaveData;
				var npcsToLoad = _allNPCs.ToList();
				foreach (var saveData in _saveData.Json)
				{
					var npc = npcsToLoad.First(n => n.GetId().ToString() == saveData.Item1);
					CharacterSaveUtility.Load(saveData.Item2, npc);
					npcsToLoad.Remove(npc);
				}
			}
			else
			{
				SaveGameManager.Register(id, _saveData);
			}
			
			SaveGameManager.OnSave += UpdateSaveData;
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
			
			UpdateSaveData();
			SaveGameManager.OnSave -= UpdateSaveData;
		}

		private void DistanceCulling()
		{
			var cameraPosition = _cameraTransform.position;
			for (int i = _inactiveNPCs.Count - 1; i >= 0; i--)
			{
				var npc = _inactiveNPCs[i];
				var squareDistance = Vector3.SqrMagnitude(cameraPosition - npc.SaveData.Position);
				if (squareDistance < cullingDistance * cullingDistance && npc.currentWorldSpace == WorldStreamer.CurrentWorldSpace)
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
				var squareDistance = Vector3.SqrMagnitude(cameraPosition - npc.SaveData.Position);
				if (squareDistance > cullingDistance * cullingDistance || npc.currentWorldSpace != WorldStreamer.CurrentWorldSpace)
				{
					npc.gameObject.SetActive(false);
					npc.StopAllCoroutines();
					
					_inactiveNPCs.Add(npc);
					_activeNPCs.RemoveAt(i);
				}
			}
		}
		
		private void UpdateSaveData()
		{
			_saveData.Json?.Clear();
			_saveData.Json = new();
			foreach (var npc in _allNPCs)
			{
				var json = CharacterSaveUtility.GetSaveData(npc);
				var tuple = new Tuple<string, string>(npc.GetId().ToString(), json);
				_saveData.Json.Add(tuple);
			}
		}
	}
}