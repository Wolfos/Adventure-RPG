using System;
using Character;
using UnityEngine;


namespace Items
{
	[Serializable]
	public class EquipmentPart
	{
		public CharacterCustomizationPart Part;
		public int Index;
	}
	
	public enum EquipmentSlot
	{
		// Default value for this enum
		UNDEFINED,
		// Shoes
		Feet,
		// A shirt, chest armor, etc
		Chest,
		// Gloves
		Hands,
		// Pants, skirt, etc
		Legs,
		// Hat, helmet
		Head,
		// Shield, sword, gun
		LeftHand,
		// Shield, sword, gun
		RightHand,
		// Two-handed sword, fishing rod, big gun
		BothHands,
		Back,
		// Length of this enum
		MAX
	}
	
	[CreateAssetMenu(fileName = "New Equipment", menuName = "eeStudio/Equipment Data")]
	public class EquipmentData: ItemData
	{
		[Header("Equipment Data")]
		public EquipmentSlot EquipmentSlot;
		
		// All visual equipment pieces (except weapons) are defined in the character prefab, this defines which pieces get activated by this equipment item
		// Most clothing items are composed of multiple parts. For example, a pair of pants would affect hips, left leg, and right leg
		public EquipmentPart[] EquipmentParts;
		
		public GameObject EquippedPrefab;

		// Use the equipment prefab
		public bool UsePrefab;

		public bool OverrideAnimations;

		public AnimatorOverrideController AnimationSet;

		public int Material;

		public bool HideHair;
	}
}