using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Audio;
using Combat;
using Data;
using Interface;
using Items;
using Player;
using UnityEngine;
using UnityEngine.AI;
using Utility;
using WolfRPG.Character;
using WolfRPG.Core;
using WolfRPG.Core.Quests;
using WolfRPG.Core.Statistics;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using ItemType = WolfRPG.Inventory.ItemType;
using Random = UnityEngine.Random;

namespace Character
{
	[RequireComponent(typeof(CharacterEquipment))]
	public abstract class CharacterBase : MonoBehaviour
	{
		public ItemContainer Inventory { get; set; }

		[SerializeField, ObjectReference((int)DatabaseCategory.Characters)] public RPGObjectReference characterObjectRef;
		public CharacterComponent CharacterComponent => Data.CharacterComponent;
		public CharacterData Data { get; set; }
		public LoadoutComponent LoadoutComponent { get; set; }
		
		// Visuals
		public CharacterCustomizationData CustomizationData => CharacterComponent.VisualData;
		
		public Transform graphic;
		[SerializeField] protected Animator animator;
		[SerializeField] private float deathAnimationLength;
		[SerializeField] protected LayerMask interactionLayerMask, attackLayerMask, blockLayerMask;
		[SerializeField] private Damage unarmedDamage;
		[SerializeField] private AudioClip hitSound;
		[SerializeField] public CharacterAnimationEvents animationEvents;
		[SerializeField] protected CharacterPartPicker partPicker;
		[SerializeField] private AudioSource audioSource;
		[SerializeField] private Collider mainCollider;

		[Header("Movement speeds")]
		[SerializeField] private float generalSpeedMultiplier = 1;
		[SerializeField] private float crouchSpeedMultiplier = 0.5f;
		[SerializeField] private float blockSpeedMultiplier = 0.5f;

		protected Collider CurrentInteraction;
		
		protected Action<float, Guid> onDamaged;
		public CharacterEquipment equipment;
		private static readonly int Recoil = Animator.StringToHash("HitRecoil");
		private static readonly int Blocking = Animator.StringToHash("Blocking");
		protected bool IsInHitRecoil => animator.GetBool(Recoil);
		protected bool IsBlocking;

		private CharacterMovementStates _movementState;
		private static readonly int PetAnimalTrigger = Animator.StringToHash("PetAnimal");
		private static readonly int Height = Animator.StringToHash("Height");
		private static readonly int Width = Animator.StringToHash("Width");
		private static readonly int TalkAnimation = Animator.StringToHash("TalkAnimation");
		private static readonly int Talking = Animator.StringToHash("Talking");

		public Weapon Weapon => equipment.CurrentWeapon;

		public float SpeedMultiplier => _movementState.GetSpeedMultiplier();
		public bool StrafeMovement => _movementState.HasStrafeMovement();
		public bool IsSprinting { get; set; }

		private const float SprintStaminaDrain = 10;
		private const float StaminaRecovery = 3;
		
		private Coroutine _talkRoutine;

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
				Data.CharacterComponent.CharacterId = CharacterPool.Register(this);
			}
			else
			{
				if (Data.CharacterComponent.CharacterId != Guid.Empty) // Saved game probably hasn't loaded yet
				{
					CharacterPool.Register(Data.CharacterComponent.CharacterId, this);
				}
			}
			equipment = GetComponent<CharacterEquipment>();
			
			animationEvents.onHit += MeleeHitCallback;
			animationEvents.OnFootL += OnFootStep;
			animationEvents.OnFootR += OnFootStep;

			_movementState = new(generalSpeedMultiplier, crouchSpeedMultiplier, blockSpeedMultiplier);
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
			
		}

		protected void Update()
		{
			Data.Tick(Time.deltaTime);

			StaminaUpdate();
		}

		private void StaminaUpdate()
		{
			// Sprint stamina drain
			if (IsSprinting)
			{
				Data.Attributes.ModifyAttribute(Attribute.Stamina, -(SprintStaminaDrain * Time.deltaTime));
				if (CanSprint() == false)
				{
					StopSprint();
				}
			}
			else // Recovery
			{
				Data.Attributes.ModifyAttribute(Attribute.Stamina, (StaminaRecovery * Time.deltaTime));
			}
		}

		public void PetAnimal(float width, float height, float animationLength)
		{
			animator.SetFloat(Width, width);
			animator.SetFloat(Height, height);
			animator.SetTrigger(PetAnimalTrigger);
			
			PetAnimal(animationLength);
		}

		public void PetAnimal(float animationLength)
		{
			StartCoroutine(PetAnimalRoutine(animationLength));
		}

		protected virtual void StartPet()
		{
			
		}

		protected virtual void EndPet()
		{
			
		}

		private IEnumerator PetAnimalRoutine(float animationLength)
		{
			mainCollider.enabled = false;
			StartPet();
			
			_movementState.SetStateActive(MovementStates.Stopped);
			
			yield return new WaitForSeconds(animationLength);
			
			_movementState.SetStateInactive(MovementStates.Stopped);
			mainCollider.enabled = true;
	
			EndPet();
		}

		public bool CanSprint()
		{
			return IsBlocking == false && GetAttributeValue(Attribute.Stamina) >= 0.01f;
		}

		public void StartSprint()
		{
			if (CanSprint() == false) return;
			
			if (IsSprinting) return;
			IsSprinting = true;
			
			_movementState.SetStateActive(MovementStates.Sprinting);
		}

		public void StopSprint()
		{
			if (IsSprinting == false) return;
			IsSprinting = false;
			
			_movementState.SetStateInactive(MovementStates.Sprinting);
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
				// foreach (var target in _currentTargets)
				// {
				// 	//unarmedDamage.source = CharacterComponent.CharacterId;
				// 	//target.TakeDamage(unarmedDamage, transform.position);
				// }
			}
			//Armed
			else if(Weapon.Attacking is false)
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
			if (CurrentInteraction != null)
			{
				if (CurrentInteraction.enabled)
				{
					var interactables = CurrentInteraction.GetComponents<IInteractable>();
					foreach (var interactable in interactables)
					{
						interactable.OnInteract(this);
					}
				}
				else CurrentInteraction = null;
			}
		}
		protected void InteractionStart(Collider other)
		{
			if (((1<<other.gameObject.layer) & interactionLayerMask) != 0)
			{
				if (CurrentInteraction != null)
				{
					var currentDistance = Vector3.Distance(CurrentInteraction.transform.position, transform.position);
					var newDistance = Vector3.Distance(other.transform.position, transform.position);
					if (newDistance > currentDistance) return;
				}
				CurrentInteraction = other;
			}
		}

		protected void InteractionUpdate(Collider other)
		{
			if (other == CurrentInteraction)
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
			if (other == CurrentInteraction)
			{
				var interactables = other.GetComponents<IInteractable>();
				foreach (var interactable in interactables)
				{
					interactable.OnEndInteract(this);
				}
				CurrentInteraction = null;
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

		// We hit an enemy
		public virtual void HitEnemy(CharacterBase enemy)
		{
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
			if (CharacterComponent.Invulnerable) return;
			
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

		// I blocked an attack
		public virtual void DidBlock()
		{
		}

		// World space
		public virtual void Teleport(Vector3 position, Quaternion rotation)
		{
		}

		
		public void Talk(int talkAnimation, string text)
		{
			animator.SetInteger(TalkAnimation, Mathf.Clamp(talkAnimation, 1, 3));
			animator.SetBool(Talking, true);

			var mouthController = partPicker.mouthControllers.First(m => m.isActiveAndEnabled);
			_talkRoutine = StartCoroutine(TalkRoutine(text, mouthController));
		}

		public void StopTalk(bool stopRoutine = true)
		{
			animator.SetBool(Talking, false);
			partPicker.mouthControllers.First(m => m.isActiveAndEnabled).CloseMouth();
			
			if(stopRoutine && _talkRoutine != null) StopCoroutine(_talkRoutine);
		}
		
		private IEnumerator TalkRoutine(string text, MouthController mouthController)
		{
			for (int i = 0; i < text.Length * 0.15f; i++)
			{
				mouthController.OpenMouth();
				yield return new WaitForSeconds(Random.Range(0.1f, 0.4f));
				mouthController.CloseMouth();
				yield return new WaitForSeconds(Random.Range(0.1f, 0.3f));
			}

			StopTalk(false);
		}

		public void LookAt(Vector3 position)
		{
			var direction = (position - transform.position).normalized;
			var lookRotation = Quaternion.LookRotation(direction);
			var lr = lookRotation.eulerAngles;
			lr.x = 0;
			lr.z = 0;
			lookRotation = Quaternion.Euler(lr);
			StartCoroutine(RotateTowards(lookRotation));
		}

		private IEnumerator RotateTowards(Quaternion targetRotation)
		{
			var startRotation = graphic.rotation;
			for (float t = 0; t < 1; t += Time.deltaTime * 3)
			{
				graphic.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
				yield return null;
			}

			graphic.rotation = targetRotation;
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
			PlaySound(SoundClips.RandomFootStepRock, 0.2f);
		}
	}
}