using System;
using NPC;
using UnityEngine;

namespace Combat
{
	public enum DamageType
	{
		Pierce, Slash, Blunt, Fire
	}

	[Serializable]
	public class Damage
	{
		public DamageType type;
		public float amount;
		public float knockback = 1;
		[NonSerialized] public dynamic source;
	}
}