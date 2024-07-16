using System;
using UnityEngine;

namespace Items
{
	[Serializable]
	public class ShopItem
	{
		public ItemData item;
		public int quantity;
	}
	
	[CreateAssetMenu(fileName = "New Shop", menuName = "eeStudio/Shop", order = 1)]
	public class ShopData: ScriptableObject
	{
		public ShopItem[] shopInventory;
		public int barteringMoney = 1000;
		public PriceList priceList;
	}
}