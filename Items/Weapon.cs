using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Character;
using UnityEngine;
using Utility;

namespace Items
{
	public abstract class Weapon : Item
	{
		[Header("Weapon")]
		public Damage baseDamage;
		[SerializeField] private AudioClip attackSound;
		[SerializeField] protected LayerMask blockLayerMask;
		
		protected LayerMask AttackLayerMask;
		protected CharacterBase Character;
		public bool Attacking { get; set; }
		
		private int _defaultLayer;

		private void Awake()
		{
			onEquipped += OnEquipped;
			onUnEquipped += OnUnEquipped;
			_defaultLayer = gameObject.layer;
		}

		private void Start()
		{
			if(IsEquipped) OnEquipped(this);
		}

		private void OnDestroy()
		{
			onEquipped -= OnEquipped;
			onUnEquipped -= OnUnEquipped;
		}

		protected virtual void OnEquipped(Item item)
		{
			gameObject.SetActive(true);
			Character = container.GetComponent<CharacterBase>();
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

		public virtual void Attack(Vector3 direction, LayerMask attackLayerMask, Action onStagger)
		{
			AttackLayerMask = attackLayerMask;
			SFXPlayer.PlaySound(attackSound);
		}

		public virtual void StartBlock() { }
		public virtual void EndBlock() { }
	}
}