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
		[SerializeField] private int maxAmount;
		[SerializeField] private float respawnTime = 120;
		[SerializeField] private string npcName; 
		[SerializeField, ObjectReference((int)DatabaseCategory.Characters)] protected RPGObjectReference characterObjectRef;
		private GameObject _prefab;

		private List<NPC> _npcs;
		private Bounds _bounds;

		protected override void Start()
		{
			var characterComponent = characterObjectRef.GetComponent<CharacterComponent>();
			_prefab = characterComponent.Prefab.GetAsset<GameObject>();
			
			base.Start();
			_bounds = GetComponent<BoxCollider>().bounds;
			_npcs = new();
			InitialSpawn();
		}

		public override string Save()
		{
			var allData = "";

			foreach (var npc in _npcs)
			{
				allData += CharacterSaveUtility.GetSaveData(npc) + "</npc>";
			}

			return allData;
		}

		public override void Load(string json)
		{
			foreach (var npc in _npcs)
			{
				Destroy(npc.gameObject);
			}

			_npcs.Clear();
			StartCoroutine(LoadRoutine(json));
		}

		private IEnumerator LoadRoutine(string json)
		{
			yield return null;
			var data = json.Split(new[] {"</npc>"}, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < data.Length; i++)
			{
				var newNPC = Instantiate(_prefab, transform, false);
				var npc = newNPC.GetComponent<NPC>();
				npc.Initialize(characterObjectRef);
				newNPC.name = npcName;
				CharacterSaveUtility.Load(data[i], npc);
				npc.UpdateData();
				npc.Bounds = _bounds;
				if (npc.Data.CharacterComponent.IsDead)
				{
					newNPC.SetActive(false);
				}
				_npcs.Add(npc);
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