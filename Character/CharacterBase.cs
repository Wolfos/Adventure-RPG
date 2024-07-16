using System;
using System.Collections;
using System.Linq;
using Audio;
using Combat;
using Data;
using Interface;
using Items;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utility;
using WolfRPG.Core;
using WolfRPG.Core.Quests;
using WolfRPG.Core.Statistics;
using World;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using EquipmentData = Items.EquipmentData;
using ItemContainer = Items.ItemContainer;
using ItemData = Items.ItemData;
using ItemType = Items.ItemType;
using Random = UnityEngine.Random;

namespace Character
{
	[RequireComponent(typeof(CharacterEquipment))]
	public abstract class CharacterBase : MonoBehaviour, IDamageTaker
	{
		[FormerlySerializedAs("worldSpace")] public WorldSpace currentWorldSpace;
		public ItemContainer Inventory { get; set; }
		
		public CharacterDataObject dataObject;

		public CharacterSaveData SaveData;
		
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
		[SerializeField] private Material damageMaterial;
		[SerializeField] private new Rigidbody rigidbody;
		
		[SerializeField] protected Rigidbody[] ragdollRigidbodies;
		[SerializeField] private Collider[] ragdollColliders;
		[SerializeField] protected Transform middleSpine;

		[Header("Movement speeds")]
		[SerializeField] private float generalSpeedMultiplier = 1;
		[SerializeField] private float crouchSpeedMultiplier = 0.5f;
		[SerializeField] private float blockSpeedMultiplier = 0.5f;
		[SerializeField] private float staminaRecoveryTime = 1.0f;

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
		private float _lastStaminaUseTime;

		// TODO: Probably move these elsewhere
		private const float SprintStaminaDrain = 10;
		public const float DodgeStaminaCost = 10;
		private const float StaminaRecovery = 3;
		
		private Coroutine _talkRoutine;

		private SkinnedMeshRenderer[] _skinnedMeshRenderers;
		private MeshRenderer[] _meshRenderers;
		private Material[] _skinnedMeshRendererMaterials;
		private Material[] _meshRendererMaterials;

		private Vector3 _lastHitDirection;

		[Button("Fill ragdoll arrays")]
		private void FillRagdollArrays()
		{
			ragdollRigidbodies = animator.GetComponentsInChildren<Rigidbody>();
			ragdollColliders = animator.GetComponentsInChildren<Collider>();
			
			DisableRagdoll();
		}

		private void EnableRagdoll()
		{
			animator.enabled = false;
			animator.GetComponent<AnimationSync>().enabled = false;
			
			foreach (var rb in ragdollRigidbodies)
			{
				rb.isKinematic = false;
				var direction = _lastHitDirection * 500;
				direction.y = 400;
				rb.AddForce(direction);
			}

			foreach (var collider in ragdollColliders)
			{
				collider.enabled = true;
			}
		}

		private void DisableRagdoll()
		{
			foreach (var rb in ragdollRigidbodies)
			{
				rb.isKinematic = true;
			}

			foreach (var collider in ragdollColliders)
			{
				collider.enabled = false;
			}
		}

		public void Initialize()
		{
			Inventory = new()
			{
				Owner = this
			};
			Inventory.OnItemUsed += OnItemUsed;
			Inventory.OnItemRemoved += OnItemRemoved;
			
			if (SaveGameManager.NewGame)
			{
				SaveData = new();
				SaveData.CustomizationData = dataObject.visualData; // Make a copy for runtime

				//Data.CharacterComponent.CharacterId = CharacterPool.Register(this);
			}
			else
			{
				// if (Data.CharacterComponent.CharacterId != Guid.Empty) // Saved game probably hasn't loaded yet
				// {
				// 	CharacterPool.Register(Data.CharacterComponent.CharacterId, this);
				// }
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
			void SetRenderers()
			{
				_skinnedMeshRenderers = graphic.GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: false);
				_meshRenderers = graphic.GetComponentsInChildren<MeshRenderer>(includeInactive: false);
				_skinnedMeshRendererMaterials = new Material [_skinnedMeshRenderers.Length];
				_meshRendererMaterials = new Material [_meshRenderers.Length];
				for(var i = 0; i < _skinnedMeshRenderers.Length; i++)
				{
					_skinnedMeshRendererMaterials[i] = _skinnedMeshRenderers[i].material;
				}

				for (var i = 0; i < _meshRenderers.Length; i++)
				{
					_meshRendererMaterials[i] = _meshRenderers[i].material;
				}
			}
			if (partPicker == null)
			{
				SetRenderers();
				return;
			}

			SaveData.CustomizationData = dataObject.visualData;
			
			
			SaveData.CustomizationData.HeadCovering = 0;
			SaveData.CustomizationData.Hips = 0;
			SaveData.CustomizationData.Torso = 0;
			SaveData.CustomizationData.BackAttachment = 0;
			SaveData.CustomizationData.HandLeft = 0;
			SaveData.CustomizationData.HandRight = 0;
			SaveData.CustomizationData.LegLeft = 0;
			SaveData.CustomizationData.LegRight = 0;
			SaveData.CustomizationData.ArmLowerLeft = 0;
			SaveData.CustomizationData.ArmLowerRight = 0;
			SaveData.CustomizationData.ArmUpperLeft = 0;
			SaveData.CustomizationData.ArmUpperRight = 0;

			SaveData.CustomizationData.MaterialOverrides ??= new();
			SaveData.CustomizationData.MaterialOverrides.Clear();
			
			partPicker.EnableHair();

			// Add equipment
			foreach (var item in equipment.Equipment)
			{
				if(item.EquipmentParts == null) continue;

				if (item.HideHair)
				{
					partPicker.DisableHair();
				}
				foreach (var part in item.EquipmentParts)
				{
					CharacterCustomizationController.SetPart(part.Part, ref SaveData.CustomizationData, part.Index);
					if (item.Material != 0)
					{
						SaveData.CustomizationData.MaterialOverrides.Add(part.Part, item.Material);
					}
				}
			}
			
			CharacterCustomizationController.ApplyNewVisualData(SaveData.CustomizationData, partPicker);
			
			SetRenderers();
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
				for (int i = 0; i < dataObject.startingInventory?.Length; i++)
				{
					var itemReference = dataObject.startingInventory[i];
					var itemData = itemReference.itemData;
					
					Inventory.AddItem(itemData.Guid, itemReference.quantity);

					var equipmentData = itemData as EquipmentData;
					if (itemReference.isEquipped && equipmentData != null)
					{
						equipment.EquipItem(equipmentData);
					}
				}
			}
			
		}

		protected void Update()
		{
			StaminaUpdate();
		}

		private void StaminaUpdate()
		{
			// Sprint stamina drain
			if (IsSprinting)
			{
				var athleticsLevel = GetSkillValue(Skill.Athletics);
				ChangeStamina(-((SprintStaminaDrain - athleticsLevel * 0.1f) * Time.deltaTime));
				if (CanSprint() == false)
				{
					StopSprint();
				}
			}
			else if(Time.time - _lastStaminaUseTime >= staminaRecoveryTime) // Recovery
			{
				ChangeStamina(StaminaRecovery * Time.deltaTime);
			}
		}

		public void ChangeStamina(float change)
		{
			if(change < 0) _lastStaminaUseTime = Time.time;
			
			ChangeAttributeValue(Attribute.Stamina, change);
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

		public bool CanDodge()
		{
			return GetAttributeValue(Attribute.Stamina) >= DodgeStaminaCost;
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

		public float GetAttributeValue(Attribute attribute)
		{
			return 0;
			throw new NotImplementedException();
		}

		public void SetAttributeValue(Attribute attribute, float newValue)
		{
			return;
			throw new NotImplementedException();
		}

		public void ChangeAttributeValue(Attribute attribute, float change)
		{
			
		}

		public int GetSkillValue(Skill skill)
		{
			return 0;
			throw new NotImplementedException();
		}

		public void OnItemRemoved(ItemData item, int slotIndex, bool wasLast)
		{
			if (wasLast && equipment.IsEquipped(item.Guid))
			{
				equipment.UnequipItem(item as EquipmentData);
			}
		}

		public void OnItemUsed(ItemData item, int slot)
		{
			switch (item.Type)
			{
				case ItemType.Consumable:
					var consumable = item as ConsumableData;
					if (consumable == null)
					{
						Debug.LogError($"Item {item.name} is not a valid consumable");
						return;
					}

					var currentValue = GetAttributeValue(consumable.attribute);
					SetAttributeValue(consumable.attribute, currentValue + consumable.effect);
					Inventory.RemoveItem(item);
					break;
				case ItemType.Weapon:
				case ItemType.Equipment:
					var equipmentData = item as EquipmentData;
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

		public bool IsDead()
		{
			return GetAttributeValue(Attribute.Health) <= 0;
		}

		public void EquipItem(Guid guid)
		{
			var itemData = ItemDatabase.GetItem(guid);
			var equipmentData = itemData as EquipmentData;

			if (itemData == null || equipmentData == null)
			{
				Debug.LogError("Object is not valid equipment");
				return;
			}
			
			equipment.EquipItem(equipmentData);
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
				Weapon.Attack(graphic.forward, attackLayerMask, blockLayerMask, Stagger);
			}
		} 

		private void Stagger()
		{
			if (IsBlocking)
			{
				EndBlock();
			}
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

		public virtual bool SetHealth(float health)
		{
			SetAttributeValue(Attribute.Health, health);

			if (health <= 0)
			{
				Die();
				return true;
			}

			return false;
		}

		// We hit an enemy
		public virtual void HitDamageTaker(IDamageTaker damageTaker)
		{
		}
		
		public void Die()
		{
			StopAllCoroutines();
			SaveData.IsDead = true;
			DeathAnimationStarted();
			//animator.SetTrigger("Death");
			
			EnableRagdoll();

			foreach (var mr in _skinnedMeshRenderers)
			{
				// Occlusion culling breaks completely when ragdoll is on.
				mr.allowOcclusionWhenDynamic = false;
				mr.updateWhenOffscreen = true;
			}

			foreach (var mr in _meshRenderers)
			{
				mr.allowOcclusionWhenDynamic = false;
			}
			
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

		public void SetInvulnerable()
		{
			SaveData.IsInvulnerable = true; // TODO: Different boolean
		}

		public void SetVulnerable()
		{
			SaveData.IsInvulnerable = false; // TODO: Different boolean
		}

		private IEnumerator KnockbackRoutine(float knockBackAmount, Vector3 direction)
		{
			for (float t = 0; t < 1f; t += Time.deltaTime * 5)
			{
				transform.Translate(direction * (knockBackAmount * Time.deltaTime), Space.World);
				yield return null;
			}
		}

		public virtual void TakeDamage(float damage, float knockBackAmount, Vector3 point, CharacterBase other, Vector3 hitDirection = new())
		{
			if (SaveData.IsInvulnerable) return;

			_lastHitDirection = (transform.position - other.transform.position).normalized;
			
			if(Weapon != null) Weapon.InterruptAttack();
			if (SaveData.IsDead) return;

			var localHitDirection = (graphic.rotation * hitDirection).normalized;
			animator.SetFloat("HitDirection", localHitDirection.x);
			
			onDamaged?.Invoke(damage, other.SaveData.CharacterId);
			SFXPlayer.PlaySound(hitSound, 0.2f);

			//var knockback = (transform.position - point).normalized * (damage.knockback * 20);

			var newHealth = GetAttributeValue(Attribute.Health) - (int) damage;
			if (SetHealth(newHealth) == false)
			{
				animator.SetTrigger("Hit");
			}

			if (newHealth > 0)
			{
				StartCoroutine(KnockbackRoutine(knockBackAmount, _lastHitDirection));
			}
			Coroutiner.Instance.StartCoroutine(TakeDamageRoutine());
		}

		public bool CanTakeDamage()
		{
			return SaveData.IsDead == false;
		}

		private bool _takeDamageRoutineRunning;
		private IEnumerator TakeDamageRoutine()
		{
			if (_takeDamageRoutineRunning || damageMaterial == null)
			{
				yield break;
			}
			_takeDamageRoutineRunning = true;
			foreach (var mr in _skinnedMeshRenderers)
			{
				mr.material = damageMaterial;
			}

			foreach (var mr in _meshRenderers)
			{
				mr.material = damageMaterial;
			}

			yield return new WaitForSeconds(0.1f);

			for (var i = 0; i < _skinnedMeshRenderers.Length; i++)
			{
				_skinnedMeshRenderers[i].material = _skinnedMeshRendererMaterials[i];
			}
			for (var i = 0; i < _meshRenderers.Length; i++)
			{
				_meshRenderers[i].material = _meshRendererMaterials[i];
			}
			_takeDamageRoutineRunning = false;
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
			if (dataObject.isInvulnerable) return;
			
			Stagger(); // TODO: Calculate based on skill and blocker used.
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

		public Guid GetId()
		{
			return SaveData.CharacterId;
		}

		private void OnFootStep()
		{
			PlaySound(SoundClips.RandomFootStepRock, 0.2f);
		}

		public string GetName()
		{
			return dataObject.characterName?.Get();
		}
	}
}