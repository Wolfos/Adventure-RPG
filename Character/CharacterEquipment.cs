using System;
using System.Collections.Generic;
using Items;
using UnityEngine;
using WolfRPG.Core.Statistics;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Character
{
	public class CharacterEquipment : MonoBehaviour
	{
		private CharacterBase _characterBase;
		private ItemContainer Inventory => _characterBase.Inventory;
		[SerializeField] private Animator animator;
		[SerializeField] private CharacterPartPicker partPicker;

		public List<EquipmentData> Equipment = new();
		private Dictionary<EquipmentSlot, EquipmentData> _equipmentSlots = new();

		public void Awake()
		{
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

		public void EquipItem(EquipmentData data)
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
			
			_characterBase.UpdateCustomizationData();
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
			
			_characterBase.UpdateCustomizationData();
		}
	}
}