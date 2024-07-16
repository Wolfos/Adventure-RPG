using Items;
using UnityEngine;

namespace Character
{
	public enum NPCDemeanor
	{
		Friendly, Neutral, Hostile
	}

	public enum NPCRoutine
	{
		Idle, Wandering, Combat
	}
	
	[CreateAssetMenu(fileName = "New NPC", menuName = "eeStudio/NPCdata", order = 1)]
	public class NPCDataObject: CharacterDataObject
	{
		public NPCRoutine defaultRoutine;
		public NPCDemeanor demeanor;
		public bool isShopKeeper;
		public ShopData shop;
		public string dialogueStartNode;
	}
}