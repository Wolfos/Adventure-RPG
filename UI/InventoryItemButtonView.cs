using System;
using Items;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InventoryItemButtonView: MonoBehaviour
	{
		public Item Item { get; private set; }
		
		[SerializeField] private ItemDetailsPanel detailsPanel;
		[SerializeField] private Image image;
		[SerializeField] private Image backgroundImage;
		[SerializeField] private DraggableItem draggableItem;
		[SerializeField] private Button button;
		[SerializeField] private Text quantityText;
		[SerializeField] private Color emptyColor;
		[SerializeField] private Sprite emptySprite;

		private InventoryView _inventoryView;
		

		private void Awake()
		{
			detailsPanel.gameObject.SetActive(false);
		}

		public void Initialize(InventoryView inventoryView, int slot, int quantity, Action onClick, bool isDragable)
		{
			_inventoryView = inventoryView;
			
			draggableItem.InventoryView = inventoryView;
			draggableItem.Slot = slot;
			draggableItem.IsDragable = isDragable;
			button.onClick.AddListener(() => {onClick?.Invoke();});
			SetQuantity(quantity);
		}

		public void Select()
		{
			button.Select();
		}

		public void EquipStatusChanged(Item item, bool newStatus)
		{
			backgroundImage.color =
				newStatus ? item.equippedInventoryBackgroundColor : item.inventoryBackgroundColor;
		}

		private void SetQuantity(int quantity)
		{
			quantityText.text = "";
			if (quantity > 1)
			{
				quantityText.text = quantity.ToString();
			}
		}

		public void SetItem(Item item)
		{
			if (Item != null)
			{
				Item.onQuantityChanged -= OnQuantityChanged;
			}
			
			Item = item;
			if (item != null)
			{
				image.sprite = item.icon;
				EquipStatusChanged(item, item.IsEquipped);
				SetQuantity(item.Quantity);
				Item.onQuantityChanged += OnQuantityChanged;
			}
			else
			{
				image.sprite = emptySprite;
				backgroundImage.color = emptyColor;
				SetQuantity(0);
			}
			
			detailsPanel.SetItem(item, _inventoryView.PriceMultiplier);
		}

		private void OnQuantityChanged(Item item)
		{
			SetQuantity(item.Quantity);
		}

		public void OnPointerEnter()
		{
			if (Item != null)
			{
				detailsPanel.gameObject.SetActive(true);
			}
		}
		
		public void OnPointerExit()
		{
			detailsPanel.gameObject.SetActive(false);
		}

		private void OnDisable()
		{
			detailsPanel.gameObject.SetActive(false);
		}
	}
}