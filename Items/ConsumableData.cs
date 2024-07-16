using UnityEngine;
using WolfRPG.Core.Statistics;

namespace Items
{
	[CreateAssetMenu(fileName = "New Consumable", menuName = "eeStudio/Consumable", order = 1)]
	public class ConsumableData: ItemData
	{
		public Attribute attribute;
		public int effect;
	}
}