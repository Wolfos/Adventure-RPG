using Items;
using Models;
using Utility;
using UnityEngine;

namespace Data
{
	public class ItemDatabase : MonoBehaviour
	{
		public Item[] items;
		public float buyPriceMultiplier;
		public float sellPriceMultiplier;
		
		private void Awake()
		{
			for (var i = 0; i < items.Length; i++)
			{
				items[i].id = i;
			}
		}
	}
}