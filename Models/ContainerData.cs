using System;
using System.Collections.Generic;

namespace Models
{
	[Serializable]
	public class ContainerData
	{
		public List<int> itemIds;
		public List<int> itemQuantities;
		public int lastRefreshDay;

		public ContainerData()
		{
			itemIds = new List<int>();
			itemQuantities = new List<int>();
		}
	}
}