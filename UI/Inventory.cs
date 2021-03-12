using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Items;
using Player;
using Utility;

namespace UI
{
	/// <summary>
	/// An inventory menu
	/// </summary>
	public class Inventory : MonoBehaviour
	{
		private static Inventory instance;
		
		public Container container;

		[SerializeField]
		private GameObject itemButton;

		[SerializeField]
		private Color emptyColor;

		[SerializeField]
		private Sprite emptySprite;

		[SerializeField]
		private RectTransform itemsContainer;

		private List<Button> buttons;

		private bool firstRun = true;

		[SerializeField] private Text moneyAmount;

		private void OnEnable()
		{
			if (firstRun)
			{
				firstRun = false;
				return;
			}

			var player = SystemContainer.GetSystem<Player.Player>();
			container = player.inventory;
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
				for (int i = 0; i < buttons.Count; i++)
				{
					if (buttons[i].gameObject == EventSystem.current.currentSelectedGameObject)
					{
						container.DropItem(i);
					}
				}
			}
		}

		private void AddButtons()
		{
			buttons = new List<Button>();
			for (int i = 0; i < container.slots; i++)
			{
				GameObject button = Instantiate(itemButton);
				button.name = "ItemButton " + i;
				button.transform.SetParent(itemsContainer.transform, false);
				int iterator = i;
				button.GetComponent<Button>().onClick.AddListener(delegate { ButtonClicked(iterator); });
				if(i == 0 && InputMapper.usingController) button.GetComponent<Button>().Select();
				button.GetComponentInChildren<DraggableItem>().inventory = this;
				button.GetComponentInChildren<DraggableItem>().slot = iterator;
				Text quantityText = button.GetComponentInChildren<Text>();
				quantityText.text = "";
				if (container.items[i] != null)
				{
					int quantity = container.items[i].quantity;
					if (quantity > 1) quantityText.text = quantity.ToString();
				}
				buttons.Add(button.GetComponent<Button>());
			}
		}

		private void ClearButtons()
		{
			foreach (var b in buttons)
			{
				Destroy(b.gameObject);
			}
			
			buttons.Clear();
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
				if (item != null) ItemAdded(item);
			}
		}

		void ItemAdded(Item item)
		{
			item.onEquipped += ItemEquipped;
			item.onUnEquipped += ItemUnEquipped;

			Button button = buttons[item.slot];
			button.transform.Find("Image").GetComponent<Image>().sprite = item.icon;
			button.image.color = item.Equipped ? item.equippedInventoryBackgroundColor : item.inventoryBackgroundColor;
		}

		void ItemRemoved(Item item)
		{
			item.onEquipped -= ItemEquipped;
			item.onUnEquipped -= ItemUnEquipped;

			Button button = buttons[item.slot];
			button.image.color = emptyColor;
			button.transform.Find("Image").GetComponent<Image>().sprite = emptySprite;
		}

		void ItemEquipped(Item item)
		{
			Button button = buttons[item.slot];
			if (!button) return;
			button.image.color = item.equippedInventoryBackgroundColor;
		}

		void ItemUnEquipped(Item item)
		{
			if (item.slot >= buttons.Count) return;
			
			Button button = buttons[item.slot];
			if (!button) return;
			button.image.color = item.inventoryBackgroundColor;
		}

		void ButtonClicked(int b)
		{
			if (container.GetItemBySlot(b) == null) return; // Slot was empty
			container.GetItemBySlot(b).Equipped = !container.GetItemBySlot(b).Equipped;
		}

		/// <summary>
		/// An item was dropped, checks if it was over a button and swaps items if it was
		/// </summary>
		/// <param name="slot">The slot the item came from</param>
		public void ItemDropped(int slot)
		{
			PointerEventData pointer = new PointerEventData(EventSystem.current);
			pointer.position = Input.mousePosition;
			List<RaycastResult> results = new List<RaycastResult>();

			EventSystem.current.RaycastAll(pointer, results);

			foreach (RaycastResult result in results)
			{
				DraggableItem target = result.gameObject.GetComponent<DraggableItem>();

				if (target != null)
				{
					container.SwapItem(slot, target.slot, target.inventory.container);
					break;
				}
			}
		}
	}
}