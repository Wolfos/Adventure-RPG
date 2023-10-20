using System;
using Character;
using UnityEngine;
using System.Collections.Generic;

namespace Items
{
	public class RangedWeapon : Weapon
	{
		[HideInInspector] public Ammunition ammunition;
		
		public override bool CanAttack()
		{
			return ammunition != null;
		}
		
		public override void Attack(Vector3 direction, LayerMask attackLayerMask, LayerMask blockLayerMask, Action onStagger)
		{
			base.Attack(direction, attackLayerMask, blockLayerMask, onStagger);

			//ammunition.Fire(direction, 100, baseDamage, container.GetComponent<CharacterBase>());
		}
	}
}
