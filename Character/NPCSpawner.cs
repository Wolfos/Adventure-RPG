using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Character
{
	public class NPCSpawner : SaveableObject
	{
		[SerializeField] private string assetFile;
		[SerializeField] private int maxAmount;
		[SerializeField] private float respawnTime = 120;
		[SerializeField] private string npcName;

		private List<NPC> npcs;
		private Bounds bounds;

		protected override void Start()
		{
			base.Start();
			bounds = GetComponent<BoxCollider>().bounds;
			npcs = new List<NPC>();
			InitialSpawn();
		}

		public override string Save()
		{
			string allData = "";

			foreach (NPC npc in npcs)
			{
				allData += JsonUtility.ToJson(npc.data) + "</npc>";
			}

			return allData;
		}

		public override void Load(string json)
		{
			foreach (var npc in npcs)
			{
				Destroy(npc.gameObject);
			}
			npcs.Clear();
			var data = json.Split(new[] {"</npc>"}, StringSplitOptions.RemoveEmptyEntries);
			for (int i = 0; i < data.Length; i++)
			{
				var prefab = Resources.Load(assetFile);
				var newNPC = Instantiate(prefab, transform, false) as GameObject;
				var npc = newNPC.GetComponent<NPC>();
				newNPC.name = npcName;
				var d = JsonUtility.FromJson<CharacterData>(data[i]);
				npc.UpdateData(d);
				npc.bounds = bounds;
				if (d.isDead)
				{
					newNPC.SetActive(false);
				}
				npcs.Add(npc);
				
			}
		}

		private void InitialSpawn()
		{
			for (int i = 0; i < maxAmount; i++)
			{
				var prefab = Resources.Load(assetFile);
				var npc = Instantiate(prefab, transform, false) as GameObject;
				npc.name = npcName;

				if (npc == null)
				{
					Debug.LogError("Could not find asset file " + assetFile);
				}
				
				// Position
				var randomPosition = bounds.RandomPos();
				randomPosition.y = transform.position.y;
				npc.transform.position = randomPosition;
				
				// Object initialization
				var npcBase = npc.GetComponent<NPC>();
				npcBase.bounds = bounds;
				npcs.Add(npcBase);
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

				foreach (var npc in npcs)
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