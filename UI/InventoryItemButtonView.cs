using System;
using Items;
using UnityEngine;
using UnityEngine.UI;
using WolfRPG.Inventory;

namespace UI
{
	[RequireComponent(typeof(Button))]
	public class InventoryItemButtonView: MonoBehaviour
	{
		public ItemData Item { get; private set; }

		[SerializeField] private Text itemNameText;
		[SerializeField] private Text typeText;
		[SerializeField] private Text weightText;
		[SerializeField] private Text valueText;

		private Button _button;

		private InventoryView _inventoryView;
		

		private void Awake()
		{
			_button = GetComponent<Button>();
		}

		public void Initialize(InventoryView inventoryView, int slot, int quantity, Action onClick, bool isDragable)
		{
			_inventoryView = inventoryView;
			if(_button == null) _button = GetComponent<Button>();
			_button.onClick.AddListener(() => {onClick?.Invoke();});
			
			SetQuantity(quantity);
		}

		public void Select()
		{
			//button.Select();
		}

		public void EquipStatusChanged(Item item, bool newStatus)
		{
			// backgroundImage.color =
			// 	newStatus ? item.equippedInventoryBackgroundColor : item.inventoryBackgroundColor;
		}

		private void SetQuantity(int quantity)
		{
			// quantityText.text = "";
			// if (quantity > 1)
			// {
			// 	quantityText.text = quantity.ToString();
			// }
		}
		
		public void SetItem(ItemData item, int quantity)
		{
			if (Item != null)
			{
				//Item.onQuantityChanged -= OnQuantityChanged;
			}
			
			Item = item;
			if (item != null)
			{
				var text = quantity > 1 ? $"{item.Name.Get()} ({quantity})" : item.Name.Get();
				itemNameText.text = text;
				typeText.text = item.Type.ToString(); // TODO: Localize
				weightText.text = item.Weight.ToString("0.0");
				valueText.text = item.BaseValue.ToString();
				// image.sprite = item.icon;
				// EquipStatusChanged(item, item.IsEquipped);
				// SetQuantity(item.Quantity);
				// Item.onQuantityChanged += OnQuantityChanged;
			}
			else
			{
				// image.sprite = emptySprite;
				// backgroundImage.color = emptyColor;
				// SetQuantity(0);
			}
			
			//detailsPanel.SetItem(item, _inventoryView.PriceMultiplier);
		}

		private void OnQuantityChanged(ItemData item)
		{
			//SetQuantity(item.Quantity);
		}

		public void OnPointerEnter()
		{
			if (Item != null)
			{
				//detailsPanel.gameObject.SetActive(true);
			}
		}
		
		public void OnPointerExit()
		{
			//detailsPanel.gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			//detailsPanel.gameObject.SetActive(false);
		}
	}
}