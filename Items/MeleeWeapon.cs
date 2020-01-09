using System;
using System.Collections.Generic;
using Combat;
using NPC;
using UnityEngine;

namespace Items
{
	public class MeleeWeapon : Weapon
	{
		public LayerMask hitLayerMask;
		public override bool Attack(Vector3 direction, List<Character> targets)
		{
			foreach (var target in targets)
			{
				target.TakeDamage(baseDamage, container.transform.position);
			}

			return true;
		}
	}
}
