using System;
using UnityEngine;

namespace Items
{
	[Serializable]
	public class ItemPrice
	{
		public ItemData item;
		public int price;
	}
	
	[CreateAssetMenu(fileName = "New price list", menuName = "eeStudio/Price list", order = 1)]

	public class PriceList: ScriptableObject
	{
		public ItemPrice[] itemPrices;

		public int GetPrice(Guid itemGuid)
		{
			foreach (var item in itemPrices)
			{
				if (item.item.Guid == itemGuid)
				{
					return item.price;
				}
			}

			return -1; // Let caller decide default price
		}
	}
}