using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Data;
using Interface;
using Items;
using UI;
using UnityEngine;
using Utility;

namespace Character
{
	[RequireComponent(typeof(Container))]
	[RequireComponent(typeof(CharacterEquipment))]
	public abstract class CharacterBase : MonoBehaviour
	{
		public Container inventory;
		public CharacterData data = new CharacterData();
		public Transform graphic;
		[SerializeField] protected Animator animator;
		[SerializeField] private float startHealth;
		[SerializeField] private float healthOffset = 80;
		[SerializeField] protected float headOffset;
		[SerializeField] private float deathAnimationLength;
		[SerializeField] private CollisionCallbacks interactionTrigger, meleeAttackTrigger;
		[SerializeField] private LayerMask interactionLayerMask, attackLayerMask;
		[SerializeField] private Damage unarmedDamage;
		[SerializeField] private AudioClip hitSound;
		[SerializeField] public CharacterAnimationEvents animationEvents;

		private List<CharacterBase> currentTargets;
		private Collider currentInteraction;
		
		protected Action<Damage> onDamaged;
		public CharacterEquipment equipment;
		private static readonly int Recoil = Animator.StringToHash("HitRecoil");
		private static readonly int Blocking = Animator.StringToHash("Blocking");
		protected bool IsInHitRecoil => animator.GetBool(Recoil);
		protected bool IsBlocking;

		private Weapon Weapon => equipment.currentWeapon;
		
		protected void Awake()
		{
			if (SaveGameManager.NewGame)
			{
				data.characterId = CharacterPool.Register(this).ToString();
			}
			equipment = GetComponent<CharacterEquipment>();
			animationEvents.onHit += MeleeHitCallback;
		}

		protected void Start()
		{

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

			currentTargets = new List<CharacterBase>();
		}

		protected void OnEnable()
		{
			data.maxHealth = startHealth;
			SetHealth(startHealth);
		}

		protected void Update()
		{
			CheckTargetsAlive();
		}
		
		#region Combat
		private void CheckTargetsAlive()
		{
			List<CharacterBase> toRemove = new List<CharacterBase>();
			foreach (var target in currentTargets)
			{
				if (target == null || target.data.health <= 0)
				{
					toRemove.Add(target);
				}
			}

			foreach (var removal in toRemove)
			{
				currentTargets.Remove(removal);
			}
		}

		private void TargetTriggerEnter(Collider other)
		{
			if (((1<<other.gameObject.layer) & attackLayerMask) != 0)
			{
				var character = other.GetComponent<CharacterBase>();
				if(character != this) currentTargets.Add(character);
			}
		}

		private void TargetTriggerExit(Collider other)
		{
			if (((1 << other.gameObject.layer) & attackLayerMask) != 0)
			{
				var character = other.GetComponent<CharacterBase>();
				if (currentTargets.Contains(character))
				{
					currentTargets.Remove(character);
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
				Weapon.baseDamage.source = data.characterId;
				if (Weapon is RangedWeapon rangedWeapon)
				{
					rangedWeapon.ammunition = GetAmmo();
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
				foreach (var target in currentTargets)
				{
					unarmedDamage.source = data.characterId;
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
			if (currentInteraction != null)
			{
				if (currentInteraction.enabled)
				{
					var interactables = currentInteraction.GetComponents<IInteractable>();
					foreach (var interactable in interactables)
					{
						interactable.OnInteract(this);
					}
				}
				else currentInteraction = null;
			}
		}
		private void InteractionTriggerEnter(Collider other)
		{
			if (((1<<other.gameObject.layer) & interactionLayerMask) != 0)
			{
				if (currentInteraction != null)
				{
					var currentDistance = Vector3.Distance(currentInteraction.transform.position, transform.position);
					var newDistance = Vector3.Distance(other.transform.position, transform.position);
					if (newDistance > currentDistance) return;
				}
				currentInteraction = other;
			}
		}

		private void InteractionTriggerStay(Collider other)
		{
			if (other == currentInteraction)
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
			if (other == currentInteraction)
			{
				var interactables = other.GetComponents<IInteractable>();
				foreach (var interactable in interactables)
				{
					interactable.OnEndInteract(this);
				}
				currentInteraction = null;
			}
		}
		#endregion

		public virtual bool SetHealth(float health)
		{
			data.health = health;

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
			data.isDead = true;
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
			if (data.isDead) return;
			
			onDamaged?.Invoke(damage);
			SFXPlayer.PlaySound(hitSound, 0.2f);

			//var knockback = (transform.position - point).normalized * (damage.knockback * 20);

			if (SetHealth(data.health - damage.amount) == false)
			{
				animator.SetTrigger("Hit");
			}
		}


		/// <summary>
		/// Returns the currently equipped ammunition. If none is equipped, returns the first instead and equips that.
		/// </summary>
		protected Ammunition GetAmmo()
		{
			Item ammo = null;
			foreach (var item in inventory.items)
			{
				if (item != null && item.type == ItemType.Ammunition)
				{
					if (ammo == null || item.IsEquipped) ammo = item;
				}
			}

			if (ammo != null && !ammo.IsEquipped) ammo.IsEquipped = true;
			return ammo as Ammunition;
		}

		// My damage killed something
		public void Killed(string kill)
		{
			var quest = data.quests.FirstOrDefault(q => q.stage.target == kill);
			if (quest != null)
			{
				quest.Progress();
			}
		}
	}
}