using System.Collections.Generic;
using UnityEngine;

namespace NPC
{
	[System.Serializable]
	public class CharacterData
	{
		public Vector3 position;
		public Quaternion rotation;
		public float health;
		public Vector3 destination;
		public NPCRoutine routine;
		public Vector3 velocity;
		public List<int> items;
		public List<int> quantities;
		public List<bool> equipped;
	}
}