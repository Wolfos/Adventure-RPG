using Character;
using UI;
using UnityEngine;

namespace Items
{
	public enum ItemType
	{
		Consumable, RangedWeapon, MeleeWeapon, Ammunition, Clothing
	}
	public class Item : MonoBehaviour
	{
		[HideInInspector] public int id;
		public Color inventoryBackgroundColor = Color.white;
		public Color equippedInventoryBackgroundColor = Color.yellow;
		public string friendlyName;

		public delegate void Event(Item item);
		public Event onEquipped;
		public Event onUnEquipped;
		
		public ItemType type;
		public RuntimeAnimatorController animationSet;
		[HideInInspector] public int quantity = 1;
		public bool stackable = false;

		[HideInInspector]
		public int slot;
		private bool _equipped;

		public bool Equipped
		{
			get => _equipped;
			set
			{
				_equipped = value;
				if (value) onEquipped?.Invoke(this);
				else
				{
					onUnEquipped?.Invoke(this);
				}
				container?.EquipStatusChanged(this);
			}
		}

		[HideInInspector]
		public Container container;
		public Sprite icon;
		
		protected Rigidbody rigidbody;
		protected Collider collider;

		public void AddedToInventory(Container container, int slot)
		{
			this.slot = slot;
			if (this.container != container)
			{
				Equipped = false;
			}
			this.container = container;
			
			if (CheckRigidbody())
			{
				rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
				rigidbody.isKinematic = true;
			}

			if (CheckCollider())
			{
				collider.enabled = false;
			}
			
			gameObject.SetActive(false);
		}

		public void Drop()
		{
			transform.parent = null;
			gameObject.SetActive(true);
			
			if (CheckRigidbody())
			{
				rigidbody.isKinematic = false;
				rigidbody.useGravity = true;
				rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
			}
			
			if (CheckCollider())
			{
				collider.enabled = true;
			}

			transform.position = container.transform.position + container.transform.forward;

			container = null;
		}

		protected bool CheckRigidbody()
		{
			if(rigidbody == null) rigidbody = GetComponent<Rigidbody>();
			return rigidbody != null;
		}

		protected bool CheckCollider()
		{
			if (collider == null) collider = GetComponent<Collider>();
			return collider != null;
		}
		
		private void OnCanInteract()
		{
			Tooltip.Activate(friendlyName + (quantity > 1 ? " (" + quantity + ")" : ""), transform, Vector3.zero);
		}

		private void OnInteract(CharacterBase character)
		{
			character.inventory.AddItem(this);
			Tooltip.DeActivate();
			character.SendMessage("InteractionTriggerExit", collider);
		}
		
		private void OnEndInteract()
		{
			Tooltip.DeActivate();
		}
	}
}