using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Data;
using Items;
using Player;
using UnityEngine.InputSystem;
using Utility;
using WolfRPG.Inventory;

namespace UI
{
	public enum SelectItemBehaviour
	{
		Use,
		Transfer,
		Buy,
		Sell
	}

	public enum InventoryViewType
	{
		Normal,
		Buy,
		Sell
	}
	/// <summary>
	/// A view that displays a list of items
	/// </summary>
	public class InventoryView : MonoBehaviour
	{
		private static InventoryView instance;
		
		public ItemContainer Container;
		public ItemContainer OtherContainer;
		
		[SerializeField] private InventoryViewType type;
		[SerializeField] private InventoryItemButtonView itemButton;
		[SerializeField] private Color emptyColor;
		[SerializeField] private Sprite emptySprite;
		[SerializeField] private RectTransform itemsContainer;
		[SerializeField] private Text moneyAmount;
		[SerializeField] private SelectItemBehaviour selectItemBehaviour;

		public float PriceMultiplier { get; set; }

		private List<InventoryItemButtonView> _buttons;

		private void Awake()
		{
			var itemDatabase = Database.GetDatabase<ItemDatabase>();
			switch (type)
			{
				case InventoryViewType.Normal:
					PriceMultiplier = 1;
					break;
				case InventoryViewType.Buy:
					PriceMultiplier = itemDatabase.buyPriceMultiplier;
					break;
				case InventoryViewType.Sell:
					PriceMultiplier = itemDatabase.sellPriceMultiplier;
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		

		private void OnEnable()
		{
			EventManager.OnDrop += OnDrop;
			StartCoroutine(Enable());
		}
		
		private void OnDisable()
		{
			EventManager.OnDrop -= OnDrop;
			
			if (Container == null) return;
			
			ClearButtons();
			Container.OnItemAdded -= ItemAdded;
			Container.OnItemRemoved -= ItemRemoved;

			// foreach (var item in container.items)
			// {
			// 	if (item == null) continue;
			// 	item.onEquipped -= ItemEquipped;
			// 	item.onUnEquipped -= ItemUnEquipped;
			// }
		}
		
		
		private IEnumerator Enable()
		{
			// Wait a frame to allow container to be set by other scripts
			yield return null;
			if (Container == null)
			{
				var player = SystemContainer.GetSystem<PlayerCharacter>();
				Container = player.Inventory;
			}
			UpdateMoney();

			Container.OnItemAdded += ItemAdded;
			Container.OnItemRemoved += ItemRemoved;
			
			AddButtons();
			AddAllItems();
		}

		private void OnDrop(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled)
			{
				for (int i = 0; i < _buttons.Count; i++)
				{
					if (_buttons[i].gameObject == EventSystem.current.currentSelectedGameObject)
					{
						//container.DropItem(i);
					}
				}
			}
		}

		private void UpdateMoney()
		{
			var player = SystemContainer.GetSystem<PlayerCharacter>();
			moneyAmount.text = player.Inventory.Money.ToString("N0");
		}
		
		private void AddButtons()
		{
			_buttons = new();
			itemButton.gameObject.SetActive(false);
			for (int i = 0; i < Container.Count; i++)
			{
				var button = Instantiate(itemButton, itemsContainer.transform, false);
				var slot = i;
				button.Initialize(this, slot, 0, () => {ButtonClicked(slot);}, type == InventoryViewType.Normal);
				button.SetItem(Container.GetItemBySlot(i), Container.GetQuantityFromSlot(i));
				button.gameObject.SetActive(true);
				_buttons.Add(button);
			}
			// for (int i = 0; i < container.slots; i++)
			// {
			// 	var button = Instantiate(itemButton, itemsContainer.transform, false);
			// 	button.name = "ItemButton " + i;
			//
			// 	var slot = i;
			//
			// 	if (i == 0 && InputMapper.UsingController) button.Select();
			//
			// 	var quantity = 0;
			// 	if (container.items[i] != null)
			// 	{
			// 		quantity = container.items[i].Quantity;
			// 	}
			// 	button.Initialize(this, slot, quantity, () => {ButtonClicked(slot);}, 
			// 		type == InventoryViewType.Normal);
			// 	_buttons.Add(button);
			// }
		}

		private void ClearButtons()
		{
			foreach (var b in _buttons)
			{
				Destroy(b.gameObject);
			}
			
			_buttons.Clear();
		}

		/// <summary>
		/// Call ItemAdded for each item in our inventory
		/// This is called during initialization
		/// </summary>
		void AddAllItems()
		{
			// for (int i = 0; i < container.GetItemCount(); i++)
			// {
			// 	Item item = container.GetItemBySlot(i);
			// 	if (item != null) ItemAdded(item, item.slot);
			// }
		}

		void ItemAdded(ItemData item, int slot)
		{
			//item.onEquipped += ItemEquipped;
			//item.onUnEquipped += ItemUnEquipped;

			var button = _buttons[slot];
			button.SetItem(item, Container.GetQuantityFromSlot(slot));
			UpdateMoney();
		}

		private void ItemRemoved(ItemData item, int slot)
		{
			// item.onEquipped -= ItemEquipped;
			// item.onUnEquipped -= ItemUnEquipped;
			//
			// var button = _buttons[slot];
			// button.SetItem(null);
			// UpdateMoney();
		}

		private void ItemEquipped(Item item)
		{
			var button = _buttons[item.slot];
			if (!button) return;
			button.EquipStatusChanged(item, true);
		}

		private void ItemUnEquipped(Item item)
		{
			if (item.slot >= _buttons.Count) return;
			
			var button = _buttons[item.slot];
			if (!button) return;
			button.EquipStatusChanged(item, false);
		}

		private void ButtonClicked(int button)
		{
			if (Container.GetItemBySlot(button) == null) return; // Slot was empty
			var item = Container.GetItemBySlot(button);
			switch(selectItemBehaviour)
			{
				case SelectItemBehaviour.Use:
					//item.IsEquipped = !item.IsEquipped;
					break;
				case SelectItemBehaviour.Transfer:
					//container.MoveItem(button, otherContainer);
					break;
				case SelectItemBehaviour.Buy:
				{
					var player = SystemContainer.GetSystem<PlayerCharacter>();
					// var cost = Mathf.CeilToInt(item.basePrice * PriceMultiplier);
					// if (player.data.money >= cost)
					// {
					// 	player.data.money -= cost;
					// 	container.MoveItem(button, otherContainer);
					// }

					break;
				}
				case SelectItemBehaviour.Sell:
				{
					var player = SystemContainer.GetSystem<PlayerCharacter>();
					//player.data.money += Mathf.CeilToInt(item.basePrice * PriceMultiplier);
					UpdateMoney();
					//container.MoveItem(button, otherContainer);
					break;
				}
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		/// <summary>
		/// An item was dropped, checks if it was over a button and swaps items if it was
		/// </summary>
		/// <param name="slot">The slot the item came from</param>
		public void ItemDropped(int slot)
		{
			var pointer = new PointerEventData(EventSystem.current);
			pointer.position = InputMapper.MousePosition;
			var results = new List<RaycastResult>();

			EventSystem.current.RaycastAll(pointer, results);

			foreach (RaycastResult result in results)
			{
				var target = result.gameObject.GetComponent<DraggableItem>();

				if (target != null)
				{
					//container.SwapItem(slot, target.Slot, target.InventoryView.container);
					break;
				}
			}
		}
	}
}