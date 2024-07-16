using System;
using UnityEngine;

namespace Character
{
	public class CharacterSaveData
	{
		public CharacterSaveData()
		{
			CustomizationData.MaterialOverrides = new();
		}

		public CharacterVisualData CustomizationData;
		public NPCRoutine CurrentRoutine { get; set; }
		public Vector3 Position { get; set; }
		public Quaternion Rotation { get; set; }
		public bool IsDead { get; set; }
		public bool IsInvulnerable { get; set; }
		
		// NPC only
		public Guid CharacterId { get; set; }
		public Guid CurrentTarget { get; set; }
		
		public Vector3 Velocity { get; set; }
		public Vector3 Destination { get; set; }
	}
}