using System;
using Items;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class ItemDetailsPanel: MonoBehaviour
	{
		[SerializeField] private RectTransform rectTransform;
		[SerializeField] private Text nameText;
		[SerializeField] private Text descriptionText;
		[SerializeField] private Text priceText;

		private void OnEnable()
		{
			var halfWidth = rectTransform.sizeDelta.x / 2;
			var left = rectTransform.position.x - halfWidth;
			var right = rectTransform.position.x + halfWidth;

			if (left < 0)
			{
				var position = rectTransform.position;
				position.x = halfWidth;
				rectTransform.position = position;
			}
			else if (right > Screen.width)
			{
				var position = rectTransform.position;
				position.x = Screen.width - halfWidth;
				rectTransform.position = position;
			}
			
			var halfHeight = rectTransform.sizeDelta.y / 2;
			var top = rectTransform.position.y + halfHeight;
			var bottom = rectTransform.position.y - halfHeight;

			if (top > Screen.height || bottom < 0)
			{
				var anchoredPosition = rectTransform.anchoredPosition;
				anchoredPosition.y = -anchoredPosition.y;
				rectTransform.anchoredPosition = anchoredPosition;
			}
		}
		

		public void SetItem(Item item, float priceMultiplier)
		{
			if (item == null)
			{
				gameObject.SetActive(false);
				return;
			}
			
			//nameText.text = item.friendlyName;
			//priceText.text = Mathf.CeilToInt(item.basePrice * priceMultiplier).ToString();
		}
	}
}