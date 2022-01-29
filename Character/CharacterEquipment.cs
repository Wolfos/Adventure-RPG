using System;
using Items;
using UnityEngine;

namespace Character
{
	public class CharacterEquipment : MonoBehaviour
	{
		private CharacterBase _characterBase;
		[SerializeField] private Animator animator;
		[SerializeField] private Transform rightHand;
		[SerializeField] private Transform leftHand;
		[SerializeField] private RuntimeAnimatorController unarmed;
		[Tooltip("These objects show when the character is naked")]
		[SerializeField] private GameObject[] nakedObjects;
		[SerializeField] private Transform rootBone;
		[SerializeField] private SkinnedMeshRenderer bonesSource;
		
		private Item rightHandEquipped;
		private Item leftHandEquipped;
		private Item twoHandEquipped;
		private bool replaceEquippedItem;
		[HideInInspector] public Weapon currentWeapon;

		public void Awake()
		{
			_characterBase = GetComponent<CharacterBase>();
			_characterBase.inventory.onItemEquipped += ItemEquipped;
			_characterBase.inventory.onItemUnequipped += ItemUnequipped;
		}
		
		public void CheckEquipment()
		{
			foreach (var item in _characterBase.inventory.items)
			{
				if(item != null && item.Equipped) ItemEquipped(item);
			}
		}

		private void ItemEquipped(Item item)
		{
			var t = item.transform;
			
			switch (item.type)
			{
				case ItemType.Consumable:
					foreach (var effect in item.effects)
					{
						switch (effect.type)
						{
							case ItemEffectType.AddHealth:
								_characterBase.SetHealth(_characterBase.data.health + effect.amount);
								break;
							case ItemEffectType.AddMana:
								break;
							default:
								throw new ArgumentOutOfRangeException();
						}
					}
					_characterBase.inventory.DestroyItem(item.slot);
					break;
				case ItemType.RangedWeapon:
					if (rightHandEquipped)
					{
						replaceEquippedItem = true;
						rightHandEquipped.Equipped = false;
					}

					if (leftHandEquipped)
					{
						replaceEquippedItem = true;
						leftHandEquipped.Equipped = false;
					}
					
					if (twoHandEquipped && twoHandEquipped != item)
					{
						replaceEquippedItem = true;
						twoHandEquipped.Equipped = false;
					}

					t.parent = leftHand;
					t.localPosition = Vector3.zero;
					t.localRotation = Quaternion.identity;

					animator.runtimeAnimatorController = item.animationSet;
					twoHandEquipped = item;
					currentWeapon = item as Weapon;
					break;
				case ItemType.MeleeWeapon:
					if (rightHandEquipped && rightHandEquipped != item)
					{
						replaceEquippedItem = true;
						rightHandEquipped.Equipped = false;
					}
					
					if (twoHandEquipped)
					{
						replaceEquippedItem = true;
						twoHandEquipped.Equipped = false;
					}
					
					t.parent = rightHand;
					t.localPosition = Vector3.zero;
					t.localRotation = Quaternion.identity;

					animator.runtimeAnimatorController = item.animationSet;
					rightHandEquipped = item;
					currentWeapon = item as Weapon;
					break;
				case ItemType.Ammunition:
					break;
				case ItemType.Clothing:
					foreach (var obj in nakedObjects)
					{
						obj.SetActive(false);
					}

					var equipment = item as Equipment;
					equipment.SetBones(rootBone, bonesSource.bones);
					
					item.transform.SetParent(animator.transform);
					break;
			}
			item.gameObject.SetActive(true);
		}

		private void ItemUnequipped(Item item)
		{
			switch (item.type)
			{
				case ItemType.Consumable:
					break;
				case ItemType.RangedWeapon:
					if (!replaceEquippedItem)
					{
						animator.runtimeAnimatorController = unarmed;
						leftHandEquipped = null;
						rightHandEquipped = null;
						twoHandEquipped = null;
						currentWeapon = null;
					}

					break;
				case ItemType.MeleeWeapon:
					if (!replaceEquippedItem)
					{
						animator.runtimeAnimatorController = unarmed;
						rightHandEquipped = null;
						currentWeapon = null;
					}

					break;
				case ItemType.Ammunition:
					break;
				case ItemType.Clothing:
					foreach (var obj in nakedObjects)
					{
						obj.SetActive(true);
					}

					break;
			}
			item.gameObject.SetActive(false);
			replaceEquippedItem = false;
		}
	}
}