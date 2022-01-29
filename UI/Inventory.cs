using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Items;
using Player;
using Utility;

namespace UI
{
	public enum SelectItemBehaviour
	{
		Use,
		Transfer
	}
	/// <summary>
	/// An inventory menu
	/// </summary>
	public class Inventory : MonoBehaviour
	{
		private static Inventory instance;
		
		public Container container;
		public Container otherContainer;

		[SerializeField] private GameObject itemButton;
		[SerializeField] private Color emptyColor;
		[SerializeField] private Sprite emptySprite;
		[SerializeField] private RectTransform itemsContainer;
		[SerializeField] private Text moneyAmount;
		[SerializeField] private SelectItemBehaviour selectItemBehaviour;

		private List<Button> _buttons;
		private bool _firstRun = true;

		private void OnEnable()
		{
			if (_firstRun)
			{
				_firstRun = false;
				return;
			}

			var player = SystemContainer.GetSystem<PlayerCharacter>();
			if (container == null)
			{
				container = player.inventory;
			}

			container.onItemAdded += ItemAdded;
			container.onItemRemoved += ItemRemoved;
			moneyAmount.text = player.data.money.ToString("N0");
			
			AddButtons();
			AddAllItems();
		}

		private void OnDisable()
		{
			if (container == null) return;
			
			ClearButtons();
			container.onItemAdded -= ItemAdded;
			container.onItemRemoved -= ItemRemoved;

			foreach (var item in container.items)
			{
				if (item == null) continue;
				item.onEquipped -= ItemEquipped;
				item.onUnEquipped -= ItemUnEquipped;
			}
		}

		private void Update()
		{
			if (InputMapper.DropButton())
			{
				for (int i = 0; i < _buttons.Count; i++)
				{
					if (_buttons[i].gameObject == EventSystem.current.currentSelectedGameObject)
					{
						container.DropItem(i);
					}
				}
			}
		}

		private void AddButtons()
		{
			_buttons = new List<Button>();
			for (int i = 0; i < container.slots; i++)
			{
				var button = Instantiate(itemButton);
				button.name = "ItemButton " + i;
				button.transform.SetParent(itemsContainer.transform, false);
				int iterator = i;
				button.GetComponent<Button>().onClick.AddListener(delegate { ButtonClicked(iterator); });
				if(i == 0 && InputMapper.usingController) button.GetComponent<Button>().Select();
				button.GetComponentInChildren<DraggableItem>().inventory = this;
				button.GetComponentInChildren<DraggableItem>().slot = iterator;
				var quantityText = button.GetComponentInChildren<Text>();
				quantityText.text = "";
				if (container.items[i] != null)
				{
					int quantity = container.items[i].quantity;
					if (quantity > 1) quantityText.text = quantity.ToString();
				}
				_buttons.Add(button.GetComponent<Button>());
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

		/// <summary>
		/// Call ItemAdded for each item in our inventory
		/// This is called during initialization
		/// </summary>
		void AddAllItems()
		{
			for (int i = 0; i < container.GetItemCount(); i++)
			{
				Item item = container.GetItemBySlot(i);
				if (item != null) ItemAdded(item, item.slot);
			}
		}

		void ItemAdded(Item item, int slot)
		{
			item.onEquipped += ItemEquipped;
			item.onUnEquipped += ItemUnEquipped;

			Button button = _buttons[slot];
			button.transform.Find("Image").GetComponent<Image>().sprite = item.icon;
			button.image.color = item.Equipped ? item.equippedInventoryBackgroundColor : item.inventoryBackgroundColor;
		}

		private void ItemRemoved(Item item, int slot)
		{
			item.onEquipped -= ItemEquipped;
			item.onUnEquipped -= ItemUnEquipped;

			Button button = _buttons[slot];
			button.image.color = emptyColor;
			button.transform.Find("Image").GetComponent<Image>().sprite = emptySprite;
		}

		private void ItemEquipped(Item item)
		{
			Button button = _buttons[item.slot];
			if (!button) return;
			button.image.color = item.equippedInventoryBackgroundColor;
		}

		private void ItemUnEquipped(Item item)
		{
			if (item.slot >= _buttons.Count) return;
			
			var button = _buttons[item.slot];
			if (!button) return;
			button.image.color = item.inventoryBackgroundColor;
		}

		private void ButtonClicked(int button)
		{
			if (container.GetItemBySlot(button) == null) return; // Slot was empty
			var item = container.GetItemBySlot(button);
			switch(selectItemBehaviour)
			{
				case SelectItemBehaviour.Use:
					item.Equipped = !item.Equipped;
					break;
				case SelectItemBehaviour.Transfer:
					container.MoveItem(button, otherContainer);
					break;
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
			pointer.position = Input.mousePosition;
			var results = new List<RaycastResult>();

			EventSystem.current.RaycastAll(pointer, results);

			foreach (RaycastResult result in results)
			{
				var target = result.gameObject.GetComponent<DraggableItem>();

				if (target != null)
				{
					container.SwapItem(slot, target.slot, target.inventory.container);
					break;
				}
			}
		}
	}
}