using Items;
using Utility;
using UnityEngine;

namespace Data
{
	public class ItemDatabase : MonoBehaviour
	{
		public Item[] items;
		
		private void Awake()
		{
			for (int i = 0; i < items.Length; i++)
			{
				items[i].id = i;
			}
		}
	}
}