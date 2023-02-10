using System;
using System.Collections.Generic;
using Data;
using UnityEngine;

namespace Character
{
	[Serializable]
	public class CharacterData
	{
		public string characterId;
		public Vector3 position;
		public Quaternion rotation;
		public bool isDead;
		public float health;
		public float maxHealth;
		public Vector3 destination;
		public NPCRoutine routine;
		public Vector3 velocity;
		public List<int> items;
		public List<int> quantities;
		public List<bool> equipped;
		public string currentTarget;
		public List<QuestProgress> questProgress = new List<QuestProgress>();
		private List<Quest> _quests;

		public List<Quest> quests
		{
			get
			{
				if (_quests == null)
				{
					_quests = new List<Quest>();
					foreach (var prog in questProgress)
					{
						_quests.Add(prog.GetQuest());
					}
				}
				return _quests;
			}
		}
		public int money;
	}
}