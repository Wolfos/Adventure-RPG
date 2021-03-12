using Character;
using UnityEngine;
using System.Collections.Generic;

namespace Items
{
	public class RangedWeapon : Weapon
	{
		[HideInInspector] public Ammunition ammunition;
		public override bool Attack(Vector3 direction, List<CharacterBase> targets, LayerMask attackLayerMask)
		{
			if (ammunition == null) return false;
			base.Attack(direction, targets, attackLayerMask);

			ammunition.Fire(direction, 100, baseDamage, container.GetComponent<CharacterBase>());
			
			return true;
		}
	}
}
