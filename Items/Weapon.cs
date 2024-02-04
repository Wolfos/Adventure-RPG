using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Character;
using UnityEngine;
using Utility;
using WolfRPG.Core;
using WolfRPG.Core.Statistics;

namespace Items
{
	public abstract class Weapon : Item
	{
		[Header("Weapon")]
		[SerializeField, ObjectReference((int)DatabaseCategory.Items)] protected RPGObjectReference rpgObjectReference;
		
		protected LayerMask BlockLayerMask;
		protected LayerMask AttackLayerMask;
		public CharacterBase Character { get; set; }
		public bool Attacking { get; set; }
		public Skill AssociatedSkill { get; set; }
		
		private int _defaultLayer;

		protected AudioClip AttackSound;
		protected AudioClip HitSound;

		private void Awake()
		{
			_defaultLayer = gameObject.layer;
		}

		protected virtual void OnEquipped(Item item)
		{
			gameObject.SetActive(true);
			//Character = container.GetComponent<CharacterBase>();
			gameObject.layer = 0;
		}

		protected virtual void OnUnEquipped(Item item)
		{
			gameObject.SetActive(false);
			Character = null;
			gameObject.layer = _defaultLayer;
		}

		public void InterruptAttack()
		{
			Attacking = false;
			StopAllCoroutines();
		}

		public virtual bool CanAttack()
		{
			return !Attacking;
		}

		public virtual void Attack(Vector3 direction, LayerMask attackLayerMask, LayerMask blockLayerMask, Action onStagger)
		{
			AttackLayerMask = attackLayerMask;
			BlockLayerMask = blockLayerMask;
			SFXPlayer.PlaySound(AttackSound);
		}

		// We blocked an attack
		public void Blocked()
		{
			Character.DidBlock();
		}

		public virtual void StartBlock() { }
		public virtual void EndBlock() { }
	}
}