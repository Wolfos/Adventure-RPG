using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Character
{
	public class NPCSpawner : SaveableObject
	{
		[SerializeField] private GameObject prefab;
		[SerializeField] private int maxAmount;
		[SerializeField] private float respawnTime = 120;
		[SerializeField] private string npcName;

		private List<NPC> _npcs;
		private Bounds _bounds;

		protected override void Start()
		{
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
				var newNPC = Instantiate(prefab, transform, false);
				var npc = newNPC.GetComponent<NPC>();
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
				var npc = Instantiate(prefab, transform, false);
				npc.name = npcName + i;
				
				// Position
				var randomPosition = _bounds.RandomPos();
				randomPosition.y = transform.position.y;
				npc.transform.position = randomPosition;
				
				// Object initialization
				var npcBase = npc.GetComponent<NPC>();
				npcBase.Bounds = _bounds;
				_npcs.Add(npcBase);
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