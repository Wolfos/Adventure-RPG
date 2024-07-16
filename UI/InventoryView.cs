using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using Items;
using Player;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using Utility;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using EquipmentData = Items.EquipmentData;
using ItemType = Items.ItemType;
using PriceList = Items.PriceList;

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

	public enum InventoryViewTab
	{
		NONE, All, Weapons, Equipment, Potions, Misc, MAX
	}

	public enum ItemSort
	{
		NameAscending, NameDescending, TypeAscending, TypeDescending, WeightAscending, WeightDescending, ValueAscending, ValueDescending
	}
	
	/// <summary>
	/// A view that displays a list of items
	/// </summary>
	public class InventoryView : MonoBehaviour
	{
		public ItemContainer Container;
		public ItemContainer OtherContainer;
		public CharacterEquipment Equipment;
		
		[SerializeField] private InventoryViewType type;
		[SerializeField] private InventoryItemButtonView itemButtonPrefab;
		[SerializeField] private InventoryItemButtonView itemButtonPlaceholder;
		[SerializeField] private Color emptyColor;
		[SerializeField] private Sprite emptySprite;
		[SerializeField] private RectTransform itemsContainer;
		[SerializeField] private TextMeshProUGUI moneyAmount;
		[SerializeField] private SelectItemBehaviour selectItemBehaviour;
		[SerializeField] private TextMeshProUGUI carryWeightText;
		[SerializeField] private ItemDescription itemDescription;
		[SerializeField] private ItemSortButton[] sortButtons;

		private enum SelectElement
		{
			None, Header, Buttons
		}

		private SelectElement _selectElement;
		private int _selectionIndex;
		
		private float _lastAnalogMoveTime;
		private Vector2 _analogMoveDirection;

		private InventoryViewTab _activeTab;
		private ItemSort _activeSort;

		[SerializeField] private PlayerMenuTabComponent allTab, weaponsTab, equipmentTab, potionsTab, miscTab;

		private PriceList _priceList;

		public float PriceMultiplier { get; set; }

		private List<InventoryItemButtonView> _itemButtons;

		private void Awake()
		{
			PriceMultiplier = 1;
		}

		private void OnEnable()
		{
			EventManager.OnDrop += OnDrop;
			EventManager.OnUIMove += OnUIMove;
			EventManager.OnUIMoveAnalog += OnUIMoveAnalog;
			EventManager.OnUIConfirm += OnUIConfirm;
			EventManager.OnMenuLeft2 += OnMenuLeft2;
			EventManager.OnMenuRight2 += OnMenuRight2;
			
			StartCoroutine(Enable());
		}
		
		private void OnDisable()
		{
			EventManager.OnDrop -= OnDrop;
			EventManager.OnUIMove -= OnUIMove;
			EventManager.OnUIMoveAnalog -= OnUIMoveAnalog;
			EventManager.OnUIConfirm -= OnUIConfirm;
			EventManager.OnMenuLeft2 -= OnMenuLeft2;
			EventManager.OnMenuRight2 -= OnMenuRight2;
			
			if (Container == null) return;
			
			ClearButtons();
			Container.OnItemAdded -= ItemAdded;
			Container.OnItemRemoved -= ItemRemoved;
		}
		
		private void DisableAllTabs()
		{
			allTab.SetInactive();
			weaponsTab.SetInactive();
			equipmentTab.SetInactive();
			potionsTab.SetInactive();
			miscTab.SetInactive();
		}

		public void AllTab()
		{
			ChangeTab(InventoryViewTab.All);
		}

		public void WeaponsTab()
		{
			ChangeTab(InventoryViewTab.Weapons);
		}

		public void EquipmentTab()
		{
			ChangeTab(InventoryViewTab.Equipment);
		}
		
		public void PotionsTab()
		{
			ChangeTab(InventoryViewTab.Potions);   
		}
		
		public void MiscTab()
		{
			ChangeTab(InventoryViewTab.Misc);   
		}

		private void ShowAllItems()
		{
			foreach (var button in _itemButtons)
			{
				button.gameObject.SetActive(true);
				button.transform.SetParent(itemsContainer);
			}
		}

		private void FilterItems(InventoryViewTab tab)
		{
			ShowAllItems();
			foreach (var button in _itemButtons)
			{
				switch (tab)
				{
					case InventoryViewTab.Weapons:
						if (button.Item.Type != ItemType.Weapon)
						{
							button.gameObject.SetActive(false);
							button.transform.SetParent(itemsContainer.parent);
						}
						break;
					case InventoryViewTab.Equipment:
						if (button.Item.Type != ItemType.Equipment)
						{
							button.gameObject.SetActive(false);
							button.transform.SetParent(itemsContainer.parent);
						}
						break;
					case InventoryViewTab.Potions:
						if (button.Item.Type != ItemType.Consumable)
						{
							button.gameObject.SetActive(false);
							button.transform.SetParent(itemsContainer.parent);
						}
						break;
					case InventoryViewTab.Misc:
						if (button.Item.Type is ItemType.Weapon or ItemType.Equipment or ItemType.Consumable)
						{
							button.gameObject.SetActive(false);
							button.transform.SetParent(itemsContainer.parent);
						}
						break;
				}
			}
			
			SortItems(_activeSort);
		}
        
		public void ChangeTab(InventoryViewTab tab, bool reactivate = false)
		{
			if (reactivate == false && tab == _activeTab) return;
			_activeTab = tab;
			DisableAllTabs();
			FilterItems(tab);
            
			switch (tab)
			{
				case InventoryViewTab.NONE:
					break;
				case InventoryViewTab.All:
					allTab.SetActive();
					break;
				case InventoryViewTab.Weapons:
					weaponsTab.SetActive();
					break;
				case InventoryViewTab.Equipment:
					equipmentTab.SetActive();
					break;
				case InventoryViewTab.Potions:
					potionsTab.SetActive();
					break;
				case InventoryViewTab.Misc:
					miscTab.SetActive();
					break;
			}
		}

		public void SortItems(ItemSort sortType)
		{
			_activeSort = sortType;
			foreach (var sortButton in sortButtons)
			{
				sortButton.SetSortType(sortType);
			}
			IEnumerable<InventoryItemButtonView> sorted = null;
			switch (sortType)
			{
				case ItemSort.NameAscending:
					sorted = _itemButtons.OrderBy(i => i.Item.Name.Get());
					break;
				case ItemSort.NameDescending:
					sorted = _itemButtons.OrderByDescending(i => i.Item.Name.Get());
					break;
				case ItemSort.TypeAscending:
					sorted = _itemButtons.OrderBy(i => i.Item.Type.ToString());
					break;
				case ItemSort.TypeDescending:
					sorted = _itemButtons.OrderByDescending(i => i.Item.Type.ToString());
					break;
				case ItemSort.WeightAscending:
					sorted = _itemButtons.OrderBy(i => i.Item.Weight);
					break;
				case ItemSort.WeightDescending:
					sorted = _itemButtons.OrderByDescending(i => i.Item.Weight);
					break;
				case ItemSort.ValueAscending:
					sorted = _itemButtons.OrderBy(i => i.Item.BaseValue);
					break;
				case ItemSort.ValueDescending:
					sorted = _itemButtons.OrderByDescending(i => i.Item.BaseValue);
					break;
				default:
					return;
			}
			
			var sortedList = sorted.ToList();

			foreach (var button in _itemButtons)
			{
				button.transform.SetSiblingIndex(sortedList.IndexOf(button));
			}
		}
		

		private void UpdateTabs()
		{
			allTab.SetEnabled(true);
			if (_itemButtons.Any(i => i.Item.Type == ItemType.Weapon) == false)
			{
				weaponsTab.SetEnabled(false);
			}
			else
			{
				weaponsTab.SetEnabled(true);
			}
			if (_itemButtons.Any(i => i.Item.Type == ItemType.Equipment) == false)
			{
				equipmentTab.SetEnabled(false);
			}
			else
			{
				equipmentTab.SetEnabled(true);
			}
			if (_itemButtons.Any(i => i.Item.Type == ItemType.Consumable) == false)
			{
				potionsTab.SetEnabled(false);
			}
			else
			{
				potionsTab.SetEnabled(true);
			}
			if (_itemButtons.Any(i => (i.Item.Type is ItemType.Weapon or ItemType.Equipment or ItemType.Consumable) == false) == false)
			{
				miscTab.SetEnabled(false);
			}
			else
			{
				miscTab.SetEnabled(true);
			}
		}

		private bool IsTabActive(InventoryViewTab tab)
		{
			switch (tab)
			{
				case InventoryViewTab.All:
					return allTab.IsEnabled;
				case InventoryViewTab.Weapons:
					return weaponsTab.IsEnabled;
				case InventoryViewTab.Equipment:
					return equipmentTab.IsEnabled;
				case InventoryViewTab.Potions:
					return potionsTab.IsEnabled;
				case InventoryViewTab.Misc:
					return miscTab.IsEnabled;
				default:
					return false;
			}
		}
		
		private IEnumerator Enable()
		{
			if (itemButtonPlaceholder != null)
			{
				Destroy(itemButtonPlaceholder.gameObject);
			}
			yield return null; // Wait a frame to allow container to be set by other scripts
			if (Container == null)
			{
				Container = PlayerCharacter.GetInventory();
				Equipment = PlayerCharacter.GetEquipment();
			}
			UpdateMoney();

			Container.OnItemAdded += ItemAdded;
			Container.OnItemRemoved += ItemRemoved;

			_priceList = null;
			switch (type)
			{
				case InventoryViewType.Normal:
					break;
				case InventoryViewType.Buy:
					_priceList = Container.PriceList;
					break;
				case InventoryViewType.Sell:
					_priceList = OtherContainer.PriceList;
					break;
			}

			_selectElement = SelectElement.Buttons;
			_selectionIndex = 0;
			AddButtons();
			UpdateTabs(); // TODO: Also call when items changed

			UpdateCarryWeight();
			UpdateEquippedStatus();
			ChangeTab(InventoryViewTab.All);
		}

		private void OnDrop(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled)
			{
				for (int i = 0; i < _itemButtons.Count; i++)
				{
					if (_itemButtons[i].gameObject == EventSystem.current.currentSelectedGameObject)
					{
						//container.DropItem(i);
					}
				}
			}
		}

		private void UpdateMoney()
		{
			if (moneyAmount == null) return;
			var amount = Container.Money.ToString("N0");
			moneyAmount.text = "Money: " + amount;
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
			_itemButtons = new();
			itemButtonPrefab.gameObject.SetActive(false);

			for (int i = 0; i < Container.ItemCount; i++)
			{
				var button = Instantiate(itemButtonPrefab, itemsContainer.transform, false);
				var slot = i;
				button.Initialize(this, slot, 0, () => {ButtonClicked(slot);}, type == InventoryViewType.Normal);
				button.SetItem(Container.GetItemBySlot(i), Container.GetQuantityFromSlot(i), _priceList);
				button.gameObject.SetActive(true);
				_itemButtons.Add(button);
			}

			if (_activeTab != InventoryViewTab.NONE)
			{
				ChangeTab(_activeTab, true);
			}

			SortItems(_activeSort);
			
			if (InputMapper.UsingController)
			{
				StartCoroutine(SelectRoutine());
			}
		}

		private IEnumerator SelectRoutine()
		{
			yield return null;
			Select(_selectElement, _selectionIndex, false);
		}

		private void CheckSelectedButton()
		{
			if (_itemButtons != null)
			{
				foreach (var button in _itemButtons)
				{
					if (button.IsPointerOver == false && EventSystem.current.currentSelectedGameObject != button.gameObject)
					{
						continue;
					}

					itemDescription.SetItem(button.Item);
					return;
				}
			}

			itemDescription.SetItem(null); // No button was selected
		}

		private void Update()
		{
			CheckSelectedButton();

			if (InputMapper.UsingController)
			{
				if (_analogMoveDirection.magnitude > 0.5f && 
				    Time.unscaledTime - _lastAnalogMoveTime > 0.25f)
				{
					_lastAnalogMoveTime = Time.unscaledTime;
					Move(_analogMoveDirection);
				}
			}
		}
		private void ClearButtons()
		{
			foreach (var b in _itemButtons)
			{
				Destroy(b.gameObject);
			}
			
			_itemButtons.Clear();
		}

		void ItemAdded(ItemData item, int slot)
		{
			ClearButtons();
			AddButtons();
			UpdateMoney();
			UpdateEquippedStatus();
		}

		private void ItemRemoved(ItemData item, int slot, bool wasLast)
		{
			ClearButtons();
			AddButtons();
			UpdateEquippedStatus();
		}

		private void UpdateEquippedStatus()
		{
			if (Equipment == null) return;

			foreach (var button in _itemButtons)
			{
				var equipmentData = button.Item as EquipmentData;
				if (equipmentData != null && Equipment.Equipment.Contains(equipmentData))
				{
					button.SetEquipped();
				}
				else
				{
					button.SetNotEquipped();
				}
				
			}
		}
		

		private void ButtonClicked(int index)
		{
			if (Container.GetItemBySlot(index) == null) return; // Slot was empty
			var item = Container.GetItemBySlot(index);
			switch(selectItemBehaviour)
			{
				case SelectItemBehaviour.Use:
					Container.UseItem(item);
					
					UpdateEquippedStatus();
					UpdateCarryWeight();
					break;
				case SelectItemBehaviour.Transfer:
					var quantity = Container.GetQuantityFromSlot(index);
					
					// Transfer the whole quantity of item
					Container.RemoveItemFromSlot(index);
					OtherContainer.AddItem(item, quantity);
					break;
				case SelectItemBehaviour.Buy:
				{
					var listPrice = _priceList.GetPrice(item.Guid);
					if (listPrice == -1) listPrice = item.BaseValue; // Item wasn't on price list, pay default value
					
					var cost = Mathf.CeilToInt(listPrice * PriceMultiplier);
					if (OtherContainer.Money >= cost)
					{
						OtherContainer.Money -= cost;
						Container.Money += cost;
						UpdateMoney();
						
						// Transfer one quantity of item at a time
						Container.RemoveItem(item);
						OtherContainer.AddItem(item);
					}

					break;
				}
				case SelectItemBehaviour.Sell:
				{
					var listPrice = _priceList.GetPrice(item.Guid);
					if (listPrice == -1) listPrice = item.BaseValue; // Item wasn't on price list, pay default value
					
					var price = Mathf.CeilToInt(listPrice * PriceMultiplier);
					if (price > OtherContainer.Money) return; // Shopkeeper could not afford to buy

					Container.Money += price;
					OtherContainer.Money -= price;
					UpdateMoney();
					
					// Transfer one quantity of item at a time
					Container.RemoveItem(item);
					OtherContainer.AddItem(item);
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

		private void Deselect()
		{
			switch (_selectElement)
			{
				case SelectElement.None:
					break;
				case SelectElement.Header:
					sortButtons[_selectionIndex].OnDeselect();
					break;
				case SelectElement.Buttons:
					var itemButton = itemsContainer.transform.GetChild(_selectionIndex)
						.GetComponent<InventoryItemButtonView>();
					itemButton.OnDeselect();
					break;
			}
		}

		private void Select(SelectElement element, int index, bool deselectOld = true)
		{
			index = Mathf.Max(0, index);

			if (deselectOld)
			{
				Deselect();
			}

			switch (element)
			{
				case SelectElement.None:
					break;
				case SelectElement.Header:
					index = Mathf.Min(sortButtons.Length - 1, index);
					sortButtons[index].OnSelect();
					break;
				case SelectElement.Buttons:
					index = Mathf.Min(itemsContainer.transform.childCount - 1, index);
					var itemButton = itemsContainer.transform.GetChild(index).GetComponent<InventoryItemButtonView>();
					itemButton.OnSelect();
					break; 
			}

			_selectElement = element;
			_selectionIndex = index;
		}

		private ISelectable GetSelected()
		{
			switch (_selectElement)
			{
				case SelectElement.None:
					break;
				case SelectElement.Header:
					return sortButtons[_selectionIndex];
					break;
				case SelectElement.Buttons:
					return itemsContainer.transform.GetChild(_selectionIndex).GetComponent<InventoryItemButtonView>();
					break;
			}

			return null;
		}


		

		private void Move(Vector2 direction)
		{
			switch (_selectElement)
			{
				case SelectElement.None:
					break;
				case SelectElement.Header:
					if (direction.y < -0.5f) // Down
					{
						Select(SelectElement.Buttons, 0);
					}
					else if (direction.x < -0.5f) // Left
					{
						Select(SelectElement.Header, _selectionIndex - 1);
					}
					else if (direction.x > 0.5f) // Right
					{
						Select(SelectElement.Header, _selectionIndex + 1);
					}
					break;
				case SelectElement.Buttons:
					if (direction.y > 0.5f) // Up
					{
						if (_selectionIndex == 0)
						{
							Select(SelectElement.Header, 0);
						}
						else
						{
							Select(SelectElement.Buttons, _selectionIndex - 1);
						}
					}
					else if (direction.y < -0.5f) // Down
					{
						Select(SelectElement.Buttons, _selectionIndex + 1);
					}
					break;
			}
		}
		
		
		private void OnUIMoveAnalog(InputAction.CallbackContext context)
		{
			_analogMoveDirection = context.ReadValue<Vector2>();

			if (context.phase == InputActionPhase.Started)
			{
				_lastAnalogMoveTime = 0;
			}
		}
		
		private void OnUIMove(InputAction.CallbackContext context)
		{
			if (context.phase != InputActionPhase.Performed) return;

			Move(context.ReadValue<Vector2>());
		}

		private void OnUIConfirm(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled)
			{
				GetSelected()?.Confirm();
			}
		}
		
		
		private void OnMenuLeft2(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled)
			{
				if ((int) _activeTab > 1)
				{
					Deselect();
					for (var i = (int) _activeTab - 1; i > (int)InventoryViewTab.NONE; i--)
					{
						if (IsTabActive((InventoryViewTab) i))
						{
							ChangeTab((InventoryViewTab)i);
							break;
						}
					}
					if (_selectElement == SelectElement.Buttons)
					{
						Select(SelectElement.Buttons, 0, false);
					}
				}
			}
		}
		
		private void OnMenuRight2(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled)
			{
				if ((int) _activeTab < (int)InventoryViewTab.MAX - 1)
				{
					Deselect();
					for (var i = (int) _activeTab + 1; i < (int)InventoryViewTab.MAX; i++)
					{
						if (IsTabActive((InventoryViewTab) i))
						{
							ChangeTab((InventoryViewTab)i);
							break;
						}
					}
					if (_selectElement == SelectElement.Buttons)
					{
						Select(SelectElement.Buttons, 0, false);
					}
				}
			}
		}
	}
}