using System;
using Character;
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
		public int amount;
		public float knockback = 1;
		[NonSerialized] public string source;
	}
}