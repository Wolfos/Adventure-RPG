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
		[Space(30)] public Damage baseDamage;
		[SerializeField] private AudioClip attackSound;
		protected LayerMask attackLayerMask;

		protected CharacterBase character;
		private int defaultLayer;

		private void Awake()
		{
			onEquipped += OnEquipped;
			onUnEquipped += OnUnEquipped;
			defaultLayer = gameObject.layer;
		}

		private void Start()
		{
			if(Equipped) OnEquipped(this);
		}

		private void OnDestroy()
		{
			onEquipped -= OnEquipped;
			onUnEquipped -= OnUnEquipped;
		}

		private void OnEquipped(Item item)
		{
			gameObject.SetActive(true);
			character = container.GetComponent<CharacterBase>();
			gameObject.layer = 0;
		}

		private void OnUnEquipped(Item item)
		{
			gameObject.SetActive(false);
			character = null;
			gameObject.layer = defaultLayer;
		}

		public virtual bool Attack(Vector3 direction, List<CharacterBase> targets, LayerMask attackLayerMask)
		{
			this.attackLayerMask = attackLayerMask;
			SFXPlayer.PlaySound(attackSound);
			return true;
		}
	}
}