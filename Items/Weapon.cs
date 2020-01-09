using System.Collections;
using System.Collections.Generic;
using Combat;
using NPC;
using UnityEngine;

namespace Items
{
	public abstract class Weapon : Item
	{
		[Space(30)] public Damage baseDamage;
		
		private void Awake()
		{
			onEquipped += Equipped;
			onUnEquipped += UnEnquipped;
		}

		private void OnDestroy()
		{
			onEquipped -= Equipped;
			onUnEquipped -= UnEnquipped;
		}

		private void Equipped(Item item)
		{
			gameObject.SetActive(true);
		}

		private void UnEnquipped(Item item)
		{
			gameObject.SetActive(false);
		}

		public abstract bool Attack(Vector3 direction, List<Character> targets);
	}
}