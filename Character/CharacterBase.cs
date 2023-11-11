using System;
using System.Collections;
using System.Collections.Generic;
using Audio;
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
		public CharacterCustomizationData CustomizationData => CharacterComponent.VisualData;
		
		public Transform graphic;
		[SerializeField] protected Animator animator;
		[SerializeField] private float deathAnimationLength;
		[SerializeField] private CollisionCallbacks interactionTrigger, meleeAttackTrigger;
		[SerializeField] private LayerMask interactionLayerMask, attackLayerMask, blockLayerMask;
		[SerializeField] private Damage unarmedDamage;
		[SerializeField] private AudioClip hitSound;
		[SerializeField] public CharacterAnimationEvents animationEvents;
		[SerializeField] private CharacterPartPicker partPicker;
		[SerializeField] private AudioSource audioSource;

		[Header("Movement speeds")]
		[SerializeField] private float crouchSpeedMultiplier = 0.5f;
		[SerializeField] private float blockSpeedMultiplier = 0.5f;

		private List<CharacterBase> _currentTargets = new();
		private Collider _currentInteraction;


		protected Action<float, string> onDamaged;
		public CharacterEquipment equipment;
		private static readonly int Recoil = Animator.StringToHash("HitRecoil");
		private static readonly int Blocking = Animator.StringToHash("Blocking");
		protected bool IsInHitRecoil => animator.GetBool(Recoil);
		protected bool IsBlocking;

		private CharacterMovementStates _movementState;

		public Weapon Weapon => equipment.CurrentWeapon;

		public float SpeedMultiplier => _movementState.GetSpeedMultiplier();
		public bool StrafeMovement => _movementState.HasStrafeMovement();

		public void Initialize(RPGObjectReference characterObjectReference)
		{
			characterObjectRef = characterObjectReference;
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
			Inventory.OnItemRemoved += OnItemRemoved;
			
			if (SaveGameManager.NewGame)
			{
				Data.CharacterComponent.CharacterId = CharacterPool.Register(this).ToString();
			}
			else
			{
				if (Data.CharacterComponent.CharacterId != null) // Saved game probably hasn't loaded yet
				{
					CharacterPool.Register(Data.CharacterComponent.CharacterId, this);
				}
			}
			equipment = GetComponent<CharacterEquipment>();
			
			animationEvents.onHit += MeleeHitCallback;
			animationEvents.OnFootL += OnFootStep;
			animationEvents.OnFootR += OnFootStep;

			_movementState = new(crouchSpeedMultiplier, blockSpeedMultiplier);
		}

		private void LoadCustomizationData()
		{
			UpdateCustomizationData();
		}

		public void UpdateCustomizationData()
		{
			if (partPicker == null) return;
			
			CustomizationData.Hips = 0;
			CustomizationData.Torso = 0;
			CustomizationData.BackAttachment = 0;
			CustomizationData.HandLeft = 0;
			CustomizationData.HandRight = 0;
			CustomizationData.LegLeft = 0;
			CustomizationData.LegRight = 0;
			CustomizationData.ArmLowerLeft = 0;
			CustomizationData.ArmLowerRight = 0;
			CustomizationData.ArmUpperLeft = 0;
			CustomizationData.ArmUpperRight = 0;
			
			CustomizationData.MaterialOverrides.Clear();
			// Add equipment
			foreach (var item in equipment.Equipment)
			{
				if(item.EquipmentParts == null) continue;
				
				foreach (var part in item.EquipmentParts)
				{
					CharacterCustomizationController.SetPart(part.Part, CustomizationData, part.Index);
					if (item.Material != 0)
					{
						CustomizationData.MaterialOverrides.Add(part.Part, item.Material);
					}
				}
			}
			
			CharacterCustomizationController.SetData(CustomizationData, partPicker);
		}
		

		protected void OnDestroy()
		{
			Inventory.OnItemUsed -= OnItemUsed;
			Inventory.OnItemRemoved -= OnItemRemoved;
			
			animationEvents.onHit -= MeleeHitCallback;
			animationEvents.OnFootL -= OnFootStep;
			animationEvents.OnFootR -= OnFootStep;
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

					var itemData = itemReference.ItemObject.GetComponent<ItemData>();
					var equipmentData = itemReference.ItemObject.GetComponent<EquipmentData>();
					if (itemReference.Equipped && equipmentData != null)
					{
						equipment.EquipItem(itemData, equipmentData);
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

		public void OnItemRemoved(ItemData item, int slotIndex)
		{
			if (equipment.IsEquipped(item.RpgObject.Guid))
			{
				equipment.UnequipItem(item, item.RpgObject.GetComponent<EquipmentData>());
			}
		}

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
				case ItemType.Equipment:
					var equipmentData = itemObject.GetComponent<EquipmentData>();
					if (equipment.IsEquipped(equipmentData))
					{
						equipment.UnequipItem(item, equipmentData);
					}
					else
					{
						equipment.EquipItem(item, equipmentData);
					}
					break;
			}
		}

		public void EquipItem(string guid)
		{
			var obj = RPGDatabase.GetObject(guid);
			var itemData = obj.GetComponent<ItemData>();
			var equipmentData = obj.GetComponent<EquipmentData>();

			if (itemData == null || equipmentData == null)
			{
				Debug.LogError("Object is not valid equipment");
				return;
			}
			
			equipment.EquipItem(itemData, equipmentData);
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
			
			_movementState.SetStateActive(MovementStates.Blocking);
		}

		public void EndBlock()
		{
			IsBlocking = false;
			animator.SetBool(Blocking, false);
			if(Weapon != null) Weapon.EndBlock();
			
			_movementState.SetStateInactive(MovementStates.Blocking);
		}

		public virtual void Attack()
		{
			if (IsInHitRecoil || IsBlocking) return;
			
			bool willAttack;
			if (Weapon != null)
			{
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
					//unarmedDamage.source = CharacterComponent.CharacterId;
					//target.TakeDamage(unarmedDamage, transform.position);
				}
			}
			//Armed
			else
			{
				Weapon.Attack(graphic.forward, attackLayerMask, blockLayerMask, OnStagger);
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

		public void EndInteraction(Collider collider)
		{
			if (collider == null) return;
			
			InteractionTriggerExit(collider);
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

		public virtual void StartQuest(string guid)
		{ }

		public virtual QuestProgress GetQuestProgress(string guid)
		{
			return null;
		}

		public virtual bool HasQuest(string guid)
		{
			return false;
		}

		public void TakeDamage(float damage, Vector3 point, CharacterBase other)
		{
			if(Weapon != null) Weapon.InterruptAttack();
			if (CharacterComponent.IsDead) return;
			
			onDamaged?.Invoke(damage, other.CharacterComponent.CharacterId);
			SFXPlayer.PlaySound(hitSound, 0.2f);

			//var knockback = (transform.position - point).normalized * (damage.knockback * 20);

			if (SetHealth(GetAttributeValue(Attribute.Health) - (int)damage) == false)
			{
				animator.SetTrigger("Hit");
			}
		}
		

		// My damage killed something
		public virtual void Killed(string characterGuid)
		{
			//var quest = CharacterComponent.Quests.FirstOrDefault(q => q.CurrentStage.Target == characterID);
			// if (quest != null)
			// {
			// 	Quest.ProgressToNextStage(quest);
			// }
		}

		private void PlaySound(AudioClip clip, float volume = 1)
		{
			if (audioSource != null)
			{
				audioSource.PlayOneShot(clip, volume);
			}
		}

		private void OnFootStep()
		{
			PlaySound(SoundClips.RandomFootStepRock, 0.1f);
		}
	}
}