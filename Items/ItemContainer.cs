using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using Data;
using UnityEngine;
using WolfRPG.Core;

namespace Items
{
	public class ItemContainer
	{
		private class InventorySlot
		{
			public Guid Guid => ItemData.Guid;
			public ItemData ItemData { get; set; }
			public int Quantity { get; set; }
			public int SlotIndex { get; set; }
		}
		
		/// <summary>
		/// The amount of item slots
		/// </summary>
		public int ItemCount => _inventorySlots.Count;
		
		private IRPGDatabase _rpgDatabase;
		private List<InventorySlot> _inventorySlots = new();

		public PriceList PriceList { get; set; }
		public int Money { get; set; }
		public CharacterBase Owner { get; set; }

		public Action<ItemData, int> OnItemAdded { get; set; }
		public Action<ItemData, int, bool> OnItemRemoved { get; set; }
		public Action<ItemData, int> OnItemUsed { get; set; }
		public Action OnContentsChanged { get; set; }

		public ItemContainer()
		{
			_rpgDatabase = RPGDatabase.DefaultDatabase;
		}
		
		public ItemContainer(IRPGDatabase database)
		{
			_rpgDatabase = database;
		}

		public void UseItem(ItemData item)
		{
			var slot = _inventorySlots.FirstOrDefault(s => s.Guid == item.Guid);
			if (slot == null)
			{
				Debug.LogError("Used item was not present in this inventory");
				return;
			}
			
			OnItemUsed?.Invoke(slot.ItemData, slot.SlotIndex);
		}

		public void Clear()
		{
			_inventorySlots.Clear();
		}

		/// <summary>
		/// Add an item to the first available slot in this inventory, or increase quantity if we already have it
		/// </summary>
		/// <param name="itemObject">The item</param>
		/// <param name="quantity">Quantity of item to add</param>
		public void AddItem(ItemData itemData, int quantity = 1)
		{
			var slot = _inventorySlots.FirstOrDefault(s => s.Guid == itemData.Guid);
			if (slot != null) // Already have this item
			{
				slot.Quantity += quantity;
			}
			else
			{
				slot = new()
				{
					ItemData = itemData,
					Quantity = quantity
				};
				_inventorySlots.Add(slot);
			}

			var slotIndex = _inventorySlots.IndexOf(slot);
			OnItemAdded?.Invoke(slot.ItemData, slotIndex);
			OnContentsChanged?.Invoke();
		}

		public void RemoveItem(ItemData itemData, int quantity = 1)
		{
			var slot = _inventorySlots.FirstOrDefault(s => s.Guid == itemData.Guid);
			if (slot == null)
			{
				return;
			}

			slot.Quantity -= quantity;
			if (slot.Quantity <= 0)
			{
				_inventorySlots.Remove(slot);
			}
			
			var slotIndex = _inventorySlots.IndexOf(slot);
			OnItemRemoved?.Invoke(slot.ItemData, slotIndex, slot.Quantity == 0);
			OnContentsChanged?.Invoke();
		}

		public void RemoveItemFromSlot(int slotIndex)
		{
			var slot = _inventorySlots[slotIndex];

			_inventorySlots.Remove(slot);

			OnItemRemoved?.Invoke(slot.ItemData, slotIndex, true);
			OnContentsChanged?.Invoke();
		}
		
		/// <summary>
		/// Add an item to the first available slot in this inventory, or increase quantity if we already have it
		/// </summary>
		/// <param name="guid">The item's WolfRPG GUID</param>
		public void AddItem(Guid guid, int quantity = 1)
		{
			var itemData = ItemDatabase.GetItem(guid);
			
			if (itemData == null)
			{
				Debug.LogError("Object was not found in database");
				return;
			}
			
			AddItem(itemData, quantity);
		}

		/// <summary>
		/// Returns an item from the specific inventory slot
		/// </summary>
		/// <param name="slot"></param>
		/// <returns>The item's ItemData component</returns>
		public ItemData GetItemBySlot(int slot)
		{
			return _inventorySlots[slot].ItemData;
		}

		/// <summary>
		/// Returns an item's quantity from a specific inventory slot
		/// </summary>
		/// <param name="slot"></param>
		/// <returns>The quantity of items in the slot</returns>
		public int GetQuantityFromSlot(int slot)
		{
			return _inventorySlots[slot].Quantity;
		}

		public float GetWeight()
		{
			float weight = 0;
			foreach (var slot in _inventorySlots)
			{
				weight += slot.ItemData.Weight * slot.Quantity;
			}

			return weight;
		}
	}
}