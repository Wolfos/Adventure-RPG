using System.Collections.Generic;
using UnityEngine;
using WolfRPG.Core.Statistics;
using WolfRPG.Inventory;

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

		public void Awake()
		{
			ResetAnimations();
			
			_characterBase = GetComponent<CharacterBase>();
			for (var i = EquipmentSlot.UNDEFINED; i < EquipmentSlot.MAX; i++)
			{
				if(i is EquipmentSlot.UNDEFINED or EquipmentSlot.MAX) continue;
				
				_equipmentSlots.Add(i, null);
			}
		}
		
		public void CheckEquipment()
		{
		}

		public bool IsEquipped(EquipmentData data)
		{
			return Equipment.Contains(data);
		}

		public void SetAnimations(EquipmentData data)
		{
			if (data.OverrideAnimations)
			{
				animator.runtimeAnimatorController = data.AnimationSet.GetAsset<AnimatorOverrideController>();
			}
		}

		public void ResetAnimations()
		{
			animator.runtimeAnimatorController = unarmedAnimationSet;
		}

		public void EquipItem(ItemData itemData, EquipmentData data)
		{
			if (data.EquipmentSlot is EquipmentSlot.UNDEFINED or EquipmentSlot.MAX)
			{
				Debug.LogError("Item has invalid equipment slot");
				return;
			}
			var currentItem = _equipmentSlots[data.EquipmentSlot];
			if (currentItem != null)
			{
				UnequipItem(currentItem);
			}

			_equipmentSlots[data.EquipmentSlot] = data;
			Equipment.Add(data);
			if (data.StatusEffect != null)
			{
				_characterBase.Data.ApplyStatusEffect(data.StatusEffect);
			}

			if (data.UsePrefab && itemData.Prefab != null)
			{
				// Used for weapons, only right hand supported atm
				if (data.EquipmentSlot == EquipmentSlot.RightHand)
				{
					var prefab = itemData.Prefab.GetAsset<GameObject>();
					if (prefab != null)
					{
						_rightHandSocketObject = Instantiate(prefab, partPicker.handSocketRight);
					}
				}
			}
			
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

			var effect = data.StatusEffect?.GetComponent<StatusEffect>();
			if (effect != null)
			{
				_characterBase.Data.RemoveStatusEffect(effect.Id);
			}

			_equipmentSlots[data.EquipmentSlot] = null;
			Equipment.Remove(data);
			
			if (data.UsePrefab)
			{
				// Used for weapons, only right hand supported atm
				if (data.EquipmentSlot == EquipmentSlot.RightHand)
				{
					Destroy(_rightHandSocketObject);
				}
			}
			
			_characterBase.UpdateCustomizationData();
			
			if (data.OverrideAnimations)
			{
				ResetAnimations();
			}
		}
	}
}