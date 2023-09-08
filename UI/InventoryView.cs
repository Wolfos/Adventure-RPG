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
using Attribute = WolfRPG.Core.Statistics.Attribute;
using ItemType = WolfRPG.Inventory.ItemType;

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
		public ItemContainer Container;
		public ItemContainer OtherContainer;
		
		[SerializeField] private InventoryViewType type;
		[SerializeField] private InventoryItemButtonView itemButton;
		[SerializeField] private Color emptyColor;
		[SerializeField] private Sprite emptySprite;
		[SerializeField] private RectTransform itemsContainer;
		[SerializeField] private Text moneyAmount;
		[SerializeField] private SelectItemBehaviour selectItemBehaviour;
		[SerializeField] private Text carryWeightText;

		public float PriceMultiplier { get; set; }

		private List<InventoryItemButtonView> _buttons;

		private void Awake()
		{
			PriceMultiplier = 1;
			// var itemDatabase = Database.GetDatabase<ItemDatabase>();
			// switch (type)
			// {
			// 	case InventoryViewType.Normal:
			// 		PriceMultiplier = 1;
			// 		break;
			// 	case InventoryViewType.Buy:
			// 		PriceMultiplier = itemDatabase.buyPriceMultiplier;
			// 		break;
			// 	case InventoryViewType.Sell:
			// 		PriceMultiplier = itemDatabase.sellPriceMultiplier;
			// 		break;
			// 	default:
			// 		throw new ArgumentOutOfRangeException();
			// }
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
		}
		
		
		private IEnumerator Enable()
		{
			// Wait a frame to allow container to be set by other scripts
			yield return null;
			if (Container == null)
			{
				Container = PlayerCharacter.GetInventory();
			}
			UpdateMoney();

			Container.OnItemAdded += ItemAdded;
			Container.OnItemRemoved += ItemRemoved;
			
			AddButtons();

			UpdateCarryWeight();
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
			moneyAmount.text = Container.Money.ToString("N0");
		}

		private void UpdateCarryWeight()
		{
			if (carryWeightText != null)
			{
				var weight = Container.GetWeight();
				var maxWeight = Container.Owner?.GetAttributeValue(Attribute.MaxCarryWeight);
				carryWeightText.text = $"Weight: {weight} / {maxWeight}"; // TODO: Localize
			}
		}
		
		private void AddButtons()
		{
			_buttons = new();
			itemButton.gameObject.SetActive(false);
			for (int i = 0; i < Container.ItemCount; i++)
			{
				var button = Instantiate(itemButton, itemsContainer.transform, false);
				var slot = i;
				button.Initialize(this, slot, 0, () => {ButtonClicked(slot);}, type == InventoryViewType.Normal);
				button.SetItem(Container.GetItemBySlot(i), Container.GetQuantityFromSlot(i));
				button.gameObject.SetActive(true);
				_buttons.Add(button);
			}
		}

		private void ClearButtons()
		{
			foreach (var b in _buttons)
			{
				Destroy(b.gameObject);
			}
			
			_buttons.Clear();
		}

		void ItemAdded(ItemData item, int slot)
		{
			// TODO: Maybe optimize this
			ClearButtons();
			AddButtons();
			UpdateMoney();
		}

		private void ItemRemoved(ItemData item, int slot)
		{
			// TODO: Maybe optimize this
			ClearButtons();
			AddButtons();
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

		private void ButtonClicked(int index)
		{
			if (Container.GetItemBySlot(index) == null) return; // Slot was empty
			var item = Container.GetItemBySlot(index);
			switch(selectItemBehaviour)
			{
				case SelectItemBehaviour.Use:
					if (item.CanUse == false && item.Type != ItemType.Equipment && item.Type != ItemType.Weapon)
					{
						break;
					}
					Container.UseItem(item);
					
					UpdateCarryWeight();
					break;
				case SelectItemBehaviour.Transfer:
					var quantity = Container.GetQuantityFromSlot(index);
					
					// Transfer the whole quantity of item
					Container.RemoveItemFromSlot(index);
					OtherContainer.AddItem(item.RpgObject, quantity);
					break;
				case SelectItemBehaviour.Buy:
				{
					var cost = Mathf.CeilToInt(item.BaseValue * PriceMultiplier);
					if (OtherContainer.Money >= cost)
					{
						OtherContainer.Money -= cost;
						
						// Transfer one quantity of item at a time
						Container.RemoveItem(item.RpgObject);
						OtherContainer.AddItem(item.RpgObject);
					}

					break;
				}
				case SelectItemBehaviour.Sell:
				{
					Container.Money += Mathf.CeilToInt(item.BaseValue * PriceMultiplier);
					UpdateMoney();
					
					// Transfer one quantity of item at a time
					Container.RemoveItem(item.RpgObject);
					OtherContainer.AddItem(item.RpgObject);
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