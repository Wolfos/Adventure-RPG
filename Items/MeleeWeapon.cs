using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using UnityEngine;

namespace Items
{
	public class MeleeWeapon : Weapon
	{
		public LayerMask hitLayerMask;
		[SerializeField] private TrailRenderer attackFx;
		[SerializeField] private float attackDuration = 1;
		private bool attacking;
		private List<CharacterBase> alreadyHit = new List<CharacterBase>();
		public override bool Attack(Vector3 direction, List<CharacterBase> targets, LayerMask attackLayerMask)
		{
			if (attacking) return false;
			
			base.Attack(direction, targets, attackLayerMask);

			StartCoroutine(AttackRoutine());

			return true;
		}

		private IEnumerator AttackRoutine()
		{
			attackFx.emitting = true;
			attacking = true;
			yield return new WaitForSeconds(attackDuration);
			attackFx.emitting = false;
			attacking = false;
			alreadyHit.Clear();
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!attacking) return;
			if (((1<<other.gameObject.layer) & attackLayerMask) != 0)
			{
				var character = other.GetComponent<CharacterBase>();
				if (alreadyHit.Contains(character)) return;
				if(character != null && character != this.character) character.TakeDamage(baseDamage, transform.position);
				alreadyHit.Add(character);
			}
		}
	}
}
