using NPC;
using UnityEngine;
using System.Collections.Generic;

namespace Items
{
	public class RangedWeapon : Weapon
	{
		[HideInInspector] public Ammunition ammunition;
		public override bool Attack(Vector3 direction, List<Character> targets)
		{
			if (ammunition == null) return false;

			ammunition.Fire(direction, 100, baseDamage, container.GetComponent<Character>());
			
			return true;
		}
	}
}
