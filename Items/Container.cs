using UnityEngine;
using System.Collections.Generic;
using Data;

namespace Items
{
	/// <summary>
	/// Holds the items for an entity.
	/// This could be the player's inventory, or the contents of a chest.
	/// </summary>
	public class Container : MonoBehaviour
	{
		[HideInInspector]
		public List<Item> items;

		public delegate void Event(Item item);
		public Event onItemAdded; // Called when an item is added to this container or moved to a different slot
		public Event onItemRemoved; // Called when an item is removed from this container, or moved to a different slot
		public Event onItemEquipped;
		public Event onItemUnequipped;

		public int slots;

		void Awake()
		{
			items = new List<Item>();

			for (int i = 0; i < slots; i++)
			{
				items.Add(null);
			}
		}

		public void SetSlots(int slots)
		{
			this.slots = slots;
			for (int i = 0; i < slots; i++)
			{
				items.Add(null);
			}
		}

		/// <summary>
		/// Add an existing item to the first available slot in this inventory
		/// </summary>
		/// <param name="item">The existing item</param>
		/// <returns>False if inventory is full</returns>
		public bool AddItem(Item item)
		{
			for(int i = 0; i < items.Count; i++)
			{
				if (item.stackable)
				{
					var slottedItem = GetItemByID(item.id);
					if (slottedItem)
					{
						slottedItem.quantity++;
						onItemAdded?.Invoke(slottedItem);
						Destroy(item.gameObject);
						return true;
					}
					else if (items[i] == null)
					{
						items[i] = item;
						item.quantity = 1;
						if (item != null)
						{
							item.AddedToInventory(this, i);
							item.transform.parent = transform;
						}
						onItemAdded?.Invoke(item);
						return true;
					}
				}
				else if (items[i] == null)
				{
					items[i] = item;
					if (item != null)
					{
						item.quantity = 1;
						item.AddedToInventory(this, i);
						item.transform.parent = transform;
					}
                    onItemAdded?.Invoke(item);
                    return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Add a new item to the first available slot in this inventory
		/// </summary>
		/// <param name="item">The item number, from the ItemList.items List</param>
		/// <returns>False if inventory is full</returns>
		public bool AddItem(int item)
		{
			GameObject i = Instantiate(Database.GetDatabase<ItemDatabase>().items[item].gameObject);
			bool added = AddItem(i.GetComponent<Item>());
			if (!added) Destroy(i);
			return added;
		}

		/// <summary>
		/// Adds an existing item to a specific slot
		/// </summary>
		/// <param name="item">The existing item</param>
		/// <param name="slot">What slot to add the item to</param>
		/// <param name="overwrite">Whether to override if there's already an item in that slot</param>
		/// <returns>False if the item slot was full and overwrite was false</returns>
		public bool AddItem(Item item, int slot, bool overwrite = false)
		{
			if (overwrite || items[slot] == null)
			{
				items[slot] = item;

				if (item != null)
				{
					item.AddedToInventory(this, slot);
					item.transform.parent = transform;
				}

				onItemAdded?.Invoke(item);
				return true;
			}
			else return false;
		}

		/// <summary>
		/// Adds a new item to a specific slot
		/// </summary>
		/// <param name="item">The new item</param>
		/// <param name="slot">The item number, from the ItemList.items List</param>
		/// <param name="overwrite">Whether to override if there's already an item in that slot</param>
		/// <returns>False if the item slot was full and overwrite was false</returns>
		public bool AddItem(int item, int slot, bool overwrite = false)
		{
			GameObject i = Instantiate(Database.GetDatabase<ItemDatabase>().items[item].gameObject);
			return AddItem(i.GetComponent<Item>(), slot, overwrite);
		}

		/// <summary>
		/// Returns the Item that's in a specific slot
		/// </summary>
		/// <param name="slot">The slot the item is in</param>
		/// <returns></returns>
		public Item GetItemBySlot(int slot)
		{
			if (slot < items.Count) return items[slot];
			else return null;
		}

		public Item GetItemByID(int id)
		{
			foreach (var i in items)
			{
				if (i != null && i.id == id) return i;
			}

			return null;
		}

		/// <summary>
		/// Move an item, unless the target container is full
		/// </summary>
		/// <param name="slot">What slot is the item in?</param>
		/// <param name="target">The target container. Default = this</param>
		/// <returns>False if the target container was full</returns>
		public bool MoveItem(int slot, Container target = null)
		{
			if (target == null) target = this;

			Item item = items[slot];
			onItemRemoved?.Invoke(item);
			bool result = target.AddItem(item);
			if(result) item = null;
			return result;
		}

		/// <summary>
		/// Invokes either the 'onItemEquipped' or 'onItemUnequipped' Event
		/// </summary>
		/// <param name="item">The item whose 'equipped' status was changed</param>
		public void EquipStatusChanged(Item item)
		{
			if(item.Equipped) onItemEquipped?.Invoke(item);
			else onItemUnequipped?.Invoke(item);
		}

		/// <summary>
		/// Swap an item
		/// </summary>
		/// <param name="slot">What slot is the item in?</param>
		/// <param name="targetSlot">What slot should the item go in?</param>
		/// <param name="target">The target container. Default = this</param>
		public void SwapItem(int slot, int targetSlot, Container target = null)
		{
			if (target == null) target = this;

			Item item = items[slot];
			Item targetItem = target.items[targetSlot];

			if (targetItem != null) target.onItemRemoved?.Invoke(targetItem);

			if (item != null)
			{
				onItemRemoved?.Invoke(item);
				target.AddItem(item, targetSlot, true);
			}
			else
			{
				target.items[targetSlot] = null;
			}

			if (targetItem != null)
			{
				AddItem(targetItem, slot, true);
			}
			else
			{
				items[slot] = null;
			}
		}

		public void DestroyItem(int slot)
		{
			var item = items[slot];
			if (item == null) return;

			item.Equipped = false;
			
			item.onEquipped = null;
			item.onUnEquipped = null;

            onItemRemoved?.Invoke(item);
            Destroy(item.gameObject);
            
			items[slot] = null; // You never know with these GC languages
		}
		
		/// <summary>
		/// Removes an item from the container without destroying it
		/// </summary>
		/// <param name="slot">The slot the item is in</param>
		/// <returns>Whether there was an item in that slot</returns>
		public bool DropItem(int slot)
		{
			var item = items[slot];
			if (item == null) return false;

			item.Equipped = false;
			item.Drop();

			onItemRemoved?.Invoke(item);
			items[slot] = null; // You never know with these GC languages
			
			return true;
		}

		public int GetItemCount()
		{
			int count = 0;
			foreach (Item i in items)
			{
				if (i != null) count++;
			}
			return count;
		}
	}
}