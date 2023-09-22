using System;
using Character;
using Interface;
using UI;
using UnityEngine;

namespace Items
{
	public enum ItemType
	{
		Consumable, RangedWeapon, MeleeWeapon, Ammunition, Clothing
	}

	public enum ItemEffectType
	{
		AddHealth, AddMana
	}

	[Serializable]
	public class ItemEffect
	{
		public ItemEffectType type;
		public float amount;
	}
	
	public class Item : MonoBehaviour, IInteractable
	{
		public delegate void Event(Item item);
		public Event onEquipped;
		public Event onUnEquipped;
		public Event onQuantityChanged;
		
		public ItemType type;
		public RuntimeAnimatorController animationSet;

		private int _quantity = 1;
		public int Quantity
		{
			get
			{
				return _quantity;
			}
			set
			{
				_quantity = value;
				onQuantityChanged?.Invoke(this);
			}
		}
		public bool stackable = false;

		[HideInInspector]
		public int slot;
		private bool _isEquipped;

		public ItemEffect[] effects;

		public bool IsEquipped
		{
			get => _isEquipped;
			set
			{
				_isEquipped = value;
				if (value) onEquipped?.Invoke(this);
				else
				{
					onUnEquipped?.Invoke(this);
				}
				//container?.EquipStatusChanged(this);
			}
		}

		[HideInInspector]
		//public Container container;
		public Sprite icon;
		
		private Rigidbody _rigidbody;
		private Collider _collider;

		protected Rigidbody Rigidbody
		{
			get
			{
				if (_rigidbody == null) _rigidbody = GetComponent<Rigidbody>();
				return _rigidbody;
			}
		}
		
		protected Collider Collider
		{
			get
			{
				if (_collider == null) _collider = GetComponent<Collider>();
				return _collider;
			}
		}

		// public void AddedToInventory(Container container, int slot)
		// {
		// 	this.slot = slot;
		// 	if (this.container != container)
		// 	{
		// 		IsEquipped = false;
		// 	}
		// 	this.container = container;
		// 	
		// 	if (Rigidbody != null)
		// 	{
		// 		Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		// 		Rigidbody.isKinematic = true;
		// 	}
		//
		// 	if (Collider != null)
		// 	{
		// 		Collider.enabled = false;
		// 	}
		// 	
		// 	gameObject.SetActive(false);
		// }

		public void Drop()
		{
			transform.parent = null;
			gameObject.SetActive(true);
			
			if (Rigidbody != null)
			{
				Rigidbody.isKinematic = false;
				Rigidbody.useGravity = true;
				Rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}
			
			if (Collider != null)
			{
				Collider.enabled = true;
			}

			//transform.position = container.transform.position + container.transform.forward;

			//container = null;
		}
		
		public void OnCanInteract(CharacterBase character)
		{
			//Tooltip.Activate(friendlyName + (Quantity > 1 ? " (" + Quantity + ")" : ""), transform, Vector3.zero);
		}

		public void OnInteract(CharacterBase character)
		{
			//character.inventory.AddItem(this);
			Tooltip.DeActivate();
			character.SendMessage("InteractionTriggerExit", Collider);
		}
		
		public void OnEndInteract(CharacterBase character)
		{
			Tooltip.DeActivate();
		}
	}
}