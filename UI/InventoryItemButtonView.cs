using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Button))]
	public class InventoryItemButtonView: MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectable
	{
		public Items.ItemData Item { get; private set; }

		[SerializeField] private TextMeshProUGUI itemNameText;
		[SerializeField] private TextMeshProUGUI typeText;
		[SerializeField] private TextMeshProUGUI weightText;
		[SerializeField] private TextMeshProUGUI valueText;

		[SerializeField] private Color notEquippedColor;
		[SerializeField] private Color equippedColor;

		public bool IsPointerOver { get; private set; }

		private Button _button;

		private InventoryView _inventoryView;

		private void Awake()
		{
			_button = GetComponent<Button>();
			var navigation = _button.navigation;
			navigation.mode = Navigation.Mode.None;
			_button.navigation = navigation;
		}
		

		public void Initialize(InventoryView inventoryView, int slot, int quantity, Action onClick, bool isDragable)
		{
			_inventoryView = inventoryView;
			if(_button == null) _button = GetComponent<Button>();
			_button.onClick.AddListener(() => {onClick?.Invoke();});
			
			SetQuantity(quantity);
		}

		public void SetEquipped()
		{
			itemNameText.color = equippedColor;
			typeText.color = equippedColor;
			weightText.color = equippedColor;
			valueText.color = equippedColor;
		}

		public void SetNotEquipped()
		{
			itemNameText.color = notEquippedColor;
			typeText.color = notEquippedColor;
			weightText.color = notEquippedColor;
			valueText.color = notEquippedColor;
		}
		

		private void SetQuantity(int quantity)
		{
			// quantityText.text = "";
			// if (quantity > 1)
			// {
			// 	quantityText.text = quantity.ToString();
			// }
		}
		
		public void SetItem(Items.ItemData item, int quantity, Items.PriceList priceList)
		{
			Item = item;
			if (item != null)
			{
				string text = "Not localized";
				if (item.Name != null)
				{
					text = quantity > 1 ? $"{item.Name.Get()} ({quantity})" : item.Name.Get();
					gameObject.name = item.Name.Get();
				}

				itemNameText.text = text;	
				typeText.text = item.Type.ToString(); // TODO: Localize
				weightText.text = item.Weight.ToString("0.0");
				// TODO: Price multiplier
				var listPrice = item.BaseValue;
				if (priceList != null)
				{
					listPrice = priceList.GetPrice(item.Guid);
					if (listPrice == -1) listPrice = item.BaseValue; // Item wasn't on price list, pay default value
				}

				valueText.text = listPrice.ToString();
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
		

		public void OnPointerEnter(PointerEventData eventData)
		{
			IsPointerOver = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			IsPointerOver = false;
		}

		public void OnSelect()
		{
			_button.Select();
		}

		public void OnDeselect()
		{
			EventSystem.current.SetSelectedGameObject(null);
		}

		public void Confirm()
		{
			// This one's handled by the UI system
			//_button.onClick.Invoke();
		}
	}
}