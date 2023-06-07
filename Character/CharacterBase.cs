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
		public ItemContainer Inventory => _loadoutComponent.ItemContainer;

		[SerializeField] protected RPGObjectReference characterObjectRef;
		public CharacterComponent CharacterComponent => Data.CharacterComponent;
		public CharacterData Data { get; private set; }
		public Transform graphic;
		[SerializeField] protected Animator animator;
		[SerializeField] private float deathAnimationLength;
		[SerializeField] private CollisionCallbacks interactionTrigger, meleeAttackTrigger;
		[SerializeField] private LayerMask interactionLayerMask, attackLayerMask;
		[SerializeField] private Damage unarmedDamage;
		[SerializeField] private AudioClip hitSound;
		[SerializeField] public CharacterAnimationEvents animationEvents;

		private List<CharacterBase> _currentTargets = new();
		private Collider _currentInteraction;
		private LoadoutComponent _loadoutComponent;
		
		protected Action<Damage> onDamaged;
		public CharacterEquipment equipment;
		private static readonly int Recoil = Animator.StringToHash("HitRecoil");
		private static readonly int Blocking = Animator.StringToHash("Blocking");
		protected bool IsInHitRecoil => animator.GetBool(Recoil);
		protected bool IsBlocking;

		private Weapon Weapon => equipment.currentWeapon;
		
		protected void Awake()
		{
			Data = new CharacterData(
				characterObjectRef.GetComponent<CharacterAttributes>(),
				characterObjectRef.GetComponent<CharacterSkills>(),
				characterObjectRef.GetComponent<CharacterComponent>(),
				characterObjectRef.GetComponent<NpcComponent>()); // Can be null

			_loadoutComponent = characterObjectRef.GetComponent<LoadoutComponent>();
			_loadoutComponent.ItemContainer = new();
			Inventory.OnItemUsed += OnItemUsed;

			CharacterComponent.CharacterId = characterObjectRef.GetObject().Guid;
			
			if (SaveGameManager.NewGame)
			{
				//data.characterId = CharacterPool.Register(this).ToString();
			}
			equipment = GetComponent<CharacterEquipment>();
			animationEvents.onHit += MeleeHitCallback;
		}

		protected void OnDestroy()
		{
			Inventory.OnItemUsed -= OnItemUsed;
		}

		protected void Start()
		{
			for (int i = 0; i < _loadoutComponent.StartingInventory?.Length; i++)
			{
				Inventory.AddItem(RPGDatabase.GetObject(_loadoutComponent.StartingInventory[i]));
				//inventory.items[i].IsEquipped = true;
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
					
					if (consumable.AttributeStatusEffects != null)
					{
						foreach (var statusEffect in consumable.AttributeStatusEffects)
						{
							Data.ApplyStatusEffect(statusEffect);
						}
					}
					if (consumable.SkillStatusEffects != null)
					{
						foreach (var statusEffect in consumable.SkillStatusEffects)
						{
							Data.ApplyStatusEffect(statusEffect);
						}
					}

					Inventory.RemoveItem(itemObject);
					break;
				case ItemType.Weapon:
					break;
				case ItemType.Equipment:
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


		/// <summary>
		/// Returns the currently equipped ammunition. If none is equipped, returns the first instead and equips that.
		/// </summary>
		///  TODO: Reimplement
		// protected Ammunition GetAmmo()
		// {
		// 	Item ammo = null;
		// 	foreach (var item in inventory.items)
		// 	{
		// 		if (item != null && item.type == ItemType.Ammunition)
		// 		{
		// 			if (ammo == null || item.IsEquipped) ammo = item;
		// 		}
		// 	}
		//
		// 	if (ammo != null && !ammo.IsEquipped) ammo.IsEquipped = true;
		// 	return ammo as Ammunition;
		// }

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