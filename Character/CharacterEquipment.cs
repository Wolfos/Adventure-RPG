using System;
using System.Collections.Generic;
using Items;
using UnityEngine;
using WolfRPG.Core;
using WolfRPG.Core.Statistics;
using ItemType = Items.ItemType;

namespace Character
{
	public class CharacterEquipment : MonoBehaviour
	{
		private CharacterBase _characterBase;
		private ItemContainer Inventory => _characterBase.Inventory;
		[SerializeField] private Animator animator;
		[SerializeField] private CharacterPartPicker partPicker;
		[SerializeField] private AnimatorOverrideController unarmedAnimationSet;

		public readonly List<EquipmentData> Equipment = new();
		private readonly Dictionary<EquipmentSlot, EquipmentData> _equipmentSlots = new();
		private GameObject _rightHandSocketObject;
		public Weapon CurrentWeapon { get; private set; }
		private readonly List<Guid> _equippedGuids = new();

		private bool _hasInitialized;

		private void Awake()
		{
			if (_hasInitialized == false)
			{
				Initialize();
			}
		}

		private void Initialize()
		{
			_hasInitialized = true;
			
			ResetAnimations();
			
			_characterBase = GetComponent<CharacterBase>();
			for (var i = EquipmentSlot.UNDEFINED; i < EquipmentSlot.MAX; i++)
			{
				if(i is EquipmentSlot.UNDEFINED or EquipmentSlot.MAX) continue;
				
				_equipmentSlots.Add(i, null);
			}
		}

		public bool IsEquipped(Guid guid)
		{
			return _equippedGuids.Contains(guid);
		}

		public bool IsEquipped(EquipmentData data)
		{
			return Equipment.Contains(data);
		}

		public void SetAnimations(EquipmentData data)
		{
			if (data.OverrideAnimations)
			{
				animator.runtimeAnimatorController = data.AnimationSet;
			}
		}

		public void ResetAnimations()
		{
			if (unarmedAnimationSet == null) return;
			animator.runtimeAnimatorController = unarmedAnimationSet;
		}

		public List<Guid> GetEquippedItems()
		{
			return _equippedGuids;
		}

		public void EquipItem(EquipmentData data)
		{
			if (_hasInitialized == false)
			{
				Initialize();
			}

			if (data.EquipmentSlot is EquipmentSlot.UNDEFINED or EquipmentSlot.MAX)
			{
				Debug.LogError("Item has invalid equipment slot");
				return;
			}
			var currentItem = _equipmentSlots[data.EquipmentSlot];
			if (currentItem != null)
			{
				UnequipItem(data);
			}

			_equipmentSlots[data.EquipmentSlot] = data;
			Equipment.Add(data);

			if (data.UsePrefab && data.EquippedPrefab != null)
			{
				// Used for weapons, only right hand supported atm
				if (data.EquipmentSlot == EquipmentSlot.RightHand)
				{
					var prefab = data.EquippedPrefab;
					if (prefab != null)
					{
						_rightHandSocketObject = Instantiate(prefab, partPicker.handSocketRight);
						if (data.Type == ItemType.Weapon)
						{
							CurrentWeapon = _rightHandSocketObject.GetComponent<Weapon>();
							CurrentWeapon.Character = _characterBase;
						}
					}
				}
			}


			_equippedGuids.Add(data.Guid);

			
			
			_characterBase.UpdateCustomizationData();
			
			SetAnimations(data);
		}
		
		public void UnequipItem(EquipmentData data)
		{
			if (data.EquipmentSlot is EquipmentSlot.UNDEFINED or EquipmentSlot.MAX)
			{
				Debug.LogError("Item has invalid equipment slot");
				return;
			}

			_equipmentSlots[data.EquipmentSlot] = null;
			Equipment.Remove(data);
			
			if (data.UsePrefab)
			{
				// Used for weapons, only right hand supported atm
				if (data.EquipmentSlot == EquipmentSlot.RightHand)
				{
					Destroy(_rightHandSocketObject);
					CurrentWeapon = null;
				}
			}

			_equippedGuids.Remove(data.Guid);
			
			_characterBase.UpdateCustomizationData();
			
			if (data.OverrideAnimations && _characterBase.IsDead() == false)
			{
				ResetAnimations();
			}
		}
	}
}