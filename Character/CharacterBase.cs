using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Data;
using Interface;
using Items;
using UnityEngine;
using Utility;
using WolfRPG.Character;
using WolfRPG.Core;
using WolfRPG.Core.Quests;
using WolfRPG.Core.Statistics;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using ItemType = WolfRPG.Inventory.ItemType;

namespace Character
{
	[RequireComponent(typeof(CharacterEquipment))]
	public abstract class CharacterBase : MonoBehaviour
	{
		public ItemContainer Inventory { get; set; }

		[SerializeField, ObjectReference((int)DatabaseCategory.Characters)] protected RPGObjectReference characterObjectRef;
		public CharacterComponent CharacterComponent => Data.CharacterComponent;
		public CharacterData Data { get; set; }
		public LoadoutComponent LoadoutComponent { get; set; }
		
		// Visuals
		public CharacterCustomizationData CustomizationData { get; set; }
		
		public Transform graphic;
		[SerializeField] protected Animator animator;
		[SerializeField] private float deathAnimationLength;
		[SerializeField] private CollisionCallbacks interactionTrigger, meleeAttackTrigger;
		[SerializeField] private LayerMask interactionLayerMask, attackLayerMask;
		[SerializeField] private Damage unarmedDamage;
		[SerializeField] private AudioClip hitSound;
		[SerializeField] public CharacterAnimationEvents animationEvents;
		[SerializeField] private CharacterPartPicker partPicker;

		private List<CharacterBase> _currentTargets = new();
		private Collider _currentInteraction;
		private CharacterCustomizationComponent _characterCustomizationComponent;
		

		protected Action<Damage> onDamaged;
		public CharacterEquipment equipment;
		private static readonly int Recoil = Animator.StringToHash("HitRecoil");
		private static readonly int Blocking = Animator.StringToHash("Blocking");
		protected bool IsInHitRecoil => animator.GetBool(Recoil);
		protected bool IsBlocking;

		private Weapon Weapon => null; //equipment.currentWeapon;
		
		protected void Awake()
		{
			Data = new(
				characterObjectRef.GetComponent<CharacterAttributes>().CreateInstance(),
				characterObjectRef.GetComponent<CharacterSkills>().CreateInstance(),
				characterObjectRef.GetComponent<CharacterComponent>().CreateInstance(),
				characterObjectRef.GetComponent<NpcComponent>()?.CreateInstance()); // Can be null

			LoadoutComponent = characterObjectRef.GetComponent<LoadoutComponent>();

			Inventory = new()
			{
				Owner = Data
			};
			Inventory.OnItemUsed += OnItemUsed;

			CharacterComponent.CharacterId = characterObjectRef.GetObject().Guid;
			
			if (SaveGameManager.NewGame)
			{
				Data.CharacterComponent.CharacterId = CharacterPool.Register(this).ToString();
			}
			equipment = GetComponent<CharacterEquipment>();
			animationEvents.onHit += MeleeHitCallback;
		}

		private void LoadCustomizationData()
		{
			_characterCustomizationComponent = characterObjectRef.GetComponent<CharacterCustomizationComponent>();
			if (_characterCustomizationComponent == null) return;

			UpdateCustomizationData();
		}

		public void UpdateCustomizationData()
		{
			CustomizationData = new()
			{
				Gender = _characterCustomizationComponent.Gender,
				Hair = _characterCustomizationComponent.Hair,
				Head = _characterCustomizationComponent.Head,
				Eyebrows = _characterCustomizationComponent.Eyebrows,
				FacialHair = _characterCustomizationComponent.FacialHair,
			};

			var tempData = CustomizationData;
			// Add equipment
			foreach (var item in equipment.Equipment)
			{
				foreach (var part in item.EquipmentParts)
				{
					CharacterCustomizationController.SetPart(part.Part, ref tempData, part.Index);
				}
			}
			
			CharacterCustomizationController.SetData(tempData, partPicker);
		}
		

		protected void OnDestroy()
		{
			Inventory.OnItemUsed -= OnItemUsed;
		}

		protected void Start()
		{
			LoadCustomizationData();
			
			if (SaveGameManager.NewGame)
			{
				for (int i = 0; i < LoadoutComponent.StartingInventory?.Length; i++)
				{
					var itemReference = LoadoutComponent.StartingInventory[i];

					for (int j = 0; j < itemReference.Quantity; j++)
					{
						Inventory.AddItem(RPGDatabase.GetObject(itemReference.ItemObject.Guid));
					} 

					var equipmentData = itemReference.ItemObject.GetComponent<EquipmentData>();
					if (itemReference.Equipped && equipmentData != null)
					{
						equipment.EquipItem(equipmentData);
					}
				}
			}

			if (meleeAttackTrigger != null)
			{
				meleeAttackTrigger.onTriggerEnter += TargetTriggerEnter;
				meleeAttackTrigger.onTriggerExit += TargetTriggerExit;
			}

			if (interactionTrigger != null)
			{
				interactionTrigger.onTriggerEnter += InteractionTriggerEnter;
				interactionTrigger.onTriggerStay += InteractionTriggerStay;
				interactionTrigger.onTriggerExit += InteractionTriggerExit;
			}

			_currentTargets = new List<CharacterBase>();
		}

		protected void OnEnable()
		{
			//data.maxHealth = startHealth;
			//SetHealth();
		}

		protected void Update()
		{
			CheckTargetsAlive();
		}

		public int GetAttributeValue(Attribute attribute) => Data.GetAttributeValue(attribute);
		public int GetSkillValue(Skill skill) => Data.GetSkillValue(skill);

		public void OnItemUsed(ItemData item, int slot)
		{
			var itemObject = item.RpgObject;
			switch (item.Type)
			{
				case ItemType.Consumable:
					var consumable = itemObject.GetComponent<ConsumableData>();
					Data.ApplyStatusEffect(consumable.StatusEffect);

					Inventory.RemoveItem(itemObject);
					break;
				case ItemType.Weapon:
					break;
				case ItemType.Equipment:
					var equipmentData = itemObject.GetComponent<EquipmentData>();
					if (equipment.IsEquipped(equipmentData))
					{
						equipment.UnequipItem(equipmentData);
					}
					else
					{
						equipment.EquipItem(equipmentData);
					}
					break;
			}
		}
		
		#region Combat
		private void CheckTargetsAlive()
		{
			// Iterate in reverse for safe removal
			for (int i = _currentTargets.Count - 1; i >= 0; i--)
			{
				var target = _currentTargets[i];
				if (target == null || target.GetAttributeValue(Attribute.Health) <= 0)
				{
					_currentTargets.RemoveAt(i);
				}
			}
		}

		private void TargetTriggerEnter(Collider other)
		{
			if (((1<<other.gameObject.layer) & attackLayerMask) != 0)
			{
				var character = other.GetComponent<CharacterBase>();
				if(character != this) _currentTargets.Add(character);
			}
		}

		private void TargetTriggerExit(Collider other)
		{
			if (((1 << other.gameObject.layer) & attackLayerMask) != 0)
			{
				var character = other.GetComponent<CharacterBase>();
				if (_currentTargets.Contains(character))
				{
					_currentTargets.Remove(character);
				}
			}
		}

		public void StartBlock()
		{
			IsBlocking = true;
			animator.SetBool(Blocking, true);
			if(Weapon != null) Weapon.StartBlock();
		}

		public void EndBlock()
		{
			IsBlocking = false;
			animator.SetBool(Blocking, false);
			if(Weapon != null) Weapon.EndBlock();
		}

		public virtual void Attack()
		{
			if (IsInHitRecoil || IsBlocking) return;
			
			bool willAttack;
			if (Weapon)
			{
				Weapon.baseDamage.source = CharacterComponent.CharacterId;
				if (Weapon is RangedWeapon rangedWeapon)
				{
					//rangedWeapon.ammunition = GetAmmo();
				}

				willAttack = Weapon.CanAttack();
			}
			else // Unarmed attack
			{
				willAttack = true;
				// Wait for hit callback to apply damage
			}
			
			if(willAttack) animator.SetTrigger("Attack");
		}

		private void MeleeHitCallback()
		{
			// Unarmed
			if (Weapon == null)
			{
				foreach (var target in _currentTargets)
				{
					unarmedDamage.source = CharacterComponent.CharacterId;
					target.TakeDamage(unarmedDamage, transform.position);
				}
			}
			//Armed
			else
			{
				Weapon.Attack(graphic.forward, attackLayerMask, OnStagger);
			}
		}

		private void OnStagger()
		{
			animator.SetTrigger("Hit");
		}
		#endregion

		#region Interaction

		public void Interact()
		{
			if (_currentInteraction != null)
			{
				if (_currentInteraction.enabled)
				{
					var interactables = _currentInteraction.GetComponents<IInteractable>();
					foreach (var interactable in interactables)
					{
						interactable.OnInteract(this);
					}
				}
				else _currentInteraction = null;
			}
		}
		private void InteractionTriggerEnter(Collider other)
		{
			if (((1<<other.gameObject.layer) & interactionLayerMask) != 0)
			{
				if (_currentInteraction != null)
				{
					var currentDistance = Vector3.Distance(_currentInteraction.transform.position, transform.position);
					var newDistance = Vector3.Distance(other.transform.position, transform.position);
					if (newDistance > currentDistance) return;
				}
				_currentInteraction = other;
			}
		}

		private void InteractionTriggerStay(Collider other)
		{
			if (other == _currentInteraction)
			{
				var interactables = other.GetComponents<IInteractable>();
				foreach (var interactable in interactables)
				{
					interactable.OnCanInteract(this);
				}
			}
		}

		private void InteractionTriggerExit(Collider other)
		{
			if (other == _currentInteraction)
			{
				var interactables = other.GetComponents<IInteractable>();
				foreach (var interactable in interactables)
				{
					interactable.OnEndInteract(this);
				}
				_currentInteraction = null;
			}
		}
		#endregion

		public virtual bool SetHealth(int health)
		{
			Data.SetAttributeValue(Attribute.Health, health);

			if (health <= 0)
			{
				Die();
				return true;
			}

			return false;
		}

		public void Die()
		{
			StopAllCoroutines();
			CharacterComponent.IsDead = true;
			DeathAnimationStarted();
			animator.SetTrigger("Death");
			
			StartCoroutine(DeathAnimation());
		}

		protected abstract void DeathAnimationStarted();
		protected abstract void DeathAnimationFinished();

		private IEnumerator DeathAnimation()
		{
			yield return new WaitForSeconds(deathAnimationLength);
			DeathAnimationFinished();
		}

		public void TakeDamage(Damage damage, Vector3 point)
		{
			if(Weapon != null) Weapon.InterruptAttack();
			if (CharacterComponent.IsDead) return;
			
			onDamaged?.Invoke(damage);
			SFXPlayer.PlaySound(hitSound, 0.2f);

			//var knockback = (transform.position - point).normalized * (damage.knockback * 20);

			if (SetHealth(GetAttributeValue(Attribute.Health) - damage.amount) == false)
			{
				animator.SetTrigger("Hit");
			}
		}
		

		// My damage killed something
		public void Killed(string characterID)
		{
			var quest = CharacterComponent.Quests.FirstOrDefault(q => q.CurrentStage.Target == characterID);
			if (quest != null)
			{
				Quest.ProgressToNextStage(quest);
			}
		}
	}
}