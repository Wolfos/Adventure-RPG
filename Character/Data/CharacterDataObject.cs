using System;
using Items;
using UnityEngine;
using UnityEngine.Serialization;
using WolfRPG.Core.Localization;

namespace Character
{
	[Serializable]
	public class StartingInventory
	{
		public ItemData itemData;
		public int quantity = 1;
		public bool isEquipped;
	}

	[Serializable]
	public struct CharacterAttributes
	{
		public int strength;
		public int dexterity;
		public int agility;
		public int attunement;
		
		[HideInInspector] public int health;
		public int maxHealth;
		[HideInInspector] public int mana;
		public int maxMana;
		[HideInInspector] public int stamina;
		public int maxStamina;

		public int maxCarryWeight;
	}
	
	[Serializable]
	public struct CharacterSkills
	{
		public int level;
		public int swordplay;
		public int archery;
		public int defense;
		public int elemental;
		public int restoration;
		public int athletics;
		public int bluntWeapons;
		public int animalHandling;
	}
	
	[CreateAssetMenu(fileName = "New Character", menuName = "eeStudio/CharacterData", order = 1)]
	public class CharacterDataObject: ScriptableObject
	{
		[FormerlySerializedAs("startingEquipment")] public StartingInventory[] startingInventory;
		public GameObject prefab;
		public LocalizedString characterName;
		public bool isInvulnerable;
		public CharacterAttributes startingAttributes;
		public CharacterSkills startingSkills;
		public CharacterVisualData visualData;

	}
}