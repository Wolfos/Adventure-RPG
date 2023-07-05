using System;
using Items;
using UnityEngine;
using Attribute = WolfRPG.Core.Statistics.Attribute;

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
		[SerializeField] private bool skipAttackAnticipation = false;
		
		private Item rightHandEquipped;
		private Item leftHandEquipped;
		private Item twoHandEquipped;
		private bool replaceEquippedItem;
		[HideInInspector] public Weapon currentWeapon;

		public void Awake()
		{
			_characterBase = GetComponent<CharacterBase>();
		}
		
		public void CheckEquipment()
		{
		}
	}
}