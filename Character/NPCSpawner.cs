using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Utility;
using WolfRPG.Character;
using WolfRPG.Core;

namespace Character
{
	public class NPCSpawner : SaveableObject
	{
		private class NPCSpawnerSaveData : ISaveData
		{
			public List<string> Json { get; set; } = new();
		}
		
		[SerializeField] private int maxAmount;
		[SerializeField] private float respawnTime = 120;
		[SerializeField] private string npcName; 
		[SerializeField, ObjectReference((int)DatabaseCategory.Characters)] protected RPGObjectReference characterObjectRef;
		private GameObject _prefab;

		private List<NPC> _npcs;
		private Bounds _bounds;
		private NPCSpawnerSaveData _saveData = new();

		protected void Start()
		{
			var characterComponent = characterObjectRef.GetComponent<CharacterComponent>();
			_prefab = characterComponent.Prefab.GetAsset<GameObject>();
			
			//base.Start();
			_bounds = GetComponent<BoxCollider>().bounds;
			_npcs = new();
			InitialSpawn();

			if (SaveGameManager.HasData(id))
			{
				_saveData = SaveGameManager.GetData(id) as NPCSpawnerSaveData;
				for (int i = 0; i < _saveData.Json.Count; i++)
				{
					if (i >= _npcs.Count) break;
					
					var json = _saveData.Json[i];
					CharacterSaveUtility.Load(json, _npcs[i]);
					_npcs[i].UpdateData();
				}
			}
			else
			{
				SaveGameManager.Register(id, _saveData);
			}
			
			SaveGameManager.OnSave += UpdateSaveData;
		}

		private void OnDestroy()
		{
			SaveGameManager.OnSave -= UpdateSaveData;
		}

		private void UpdateSaveData()
		{
			// TODO: This will yield extremely low performance
			_saveData.Json.Clear();
			foreach (var npc in _npcs)
			{
				_saveData.Json.Add(CharacterSaveUtility.GetSaveData(npc));
			}
		}

		private void InitialSpawn()
		{
			for (int i = 0; i < maxAmount; i++)
			{
				var newNPC = Instantiate(_prefab, transform, false);
				newNPC.name = npcName + i;
				
				
				// Position
				var randomPosition = _bounds.RandomPos();
				randomPosition.y = transform.position.y;
				newNPC.transform.position = randomPosition;
				
				// Object initialization
				var npc = newNPC.GetComponent<NPC>();
				npc.Initialize(characterObjectRef);
				newNPC.SetActive(true);
				npc.Bounds = _bounds;
				
				_npcs.Add(npc);
			}
		}

		private void OnEnable()
		{
			StartCoroutine(SpawnRoutine());
		}

		private void OnDisable()
		{
			StopAllCoroutines();
		}

		private IEnumerator SpawnRoutine()
		{
			while (true)
			{
				yield return new WaitForSeconds(10);

				foreach (var npc in _npcs)
				{
					// if (!npc.gameObject.activeSelf && TimeManager.RealTime() > npc.data.inactiveTime + respawnTime)
					// {
					// 	Debug.Log("respawning");
					// 	npc.data.isDead = false;
					// 	npc.gameObject.SetActive(true);
					// }
				}
			}
		}
	}
}