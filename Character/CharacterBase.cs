using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Combat;
using Data;
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
		[SerializeField] private Collider collider;
		[SerializeField] private AudioClip hitSound;
		[SerializeField] private CharacterAnimationEvents animationEvents;

		private List<CharacterBase> currentTargets;
		private Collider currentInteraction;

		private HealthDisplay healthDisplay;
		protected Action<Damage> onDamaged;
		protected CharacterEquipment equipment;
		private static readonly int Recoil = Animator.StringToHash("HitRecoil");
		protected bool IsInHitRecoil => animator.GetBool(Recoil);
		
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
			healthDisplay = UIBase.GetHealthDisplay();
			healthDisplay.MaxHealth = startHealth;

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
			SetHealth(startHealth);
			if(healthDisplay != null) healthDisplay.gameObject.SetActive(false);
			collider.enabled = true;
		}

		protected void Update()
		{
			if (data.health < startHealth && data.health > 0 && !PlayerMenu.IsActive && !PauseMenu.isActive)
			{
				healthDisplay.gameObject.SetActive(true);
				healthDisplay.CurrentHealth = data.health;
				var headPos = transform.position;
				headPos.y += headOffset;
				var screenPos = Camera.main.WorldToScreenPoint(headPos);
				screenPos.y += healthOffset;
				healthDisplay.transform.position = screenPos;
			}
			else healthDisplay.gameObject.SetActive(false);
			
			CheckTargetsAlive();
		}
		
		#region Combat
		private void CheckTargetsAlive()
		{
			List<CharacterBase> toRemove = new List<CharacterBase>();
			foreach (var target in currentTargets)
			{
				if (target.data.health <= 0)
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

		protected void Attack()
		{
			if (IsInHitRecoil) return;
			
			var willAttack = false;
			if (equipment.currentWeapon)
			{
				equipment.currentWeapon.baseDamage.source = data.characterId;
				if (equipment.currentWeapon is RangedWeapon weapon)
				{
					weapon.ammunition = GetAmmo();
				}
				willAttack = equipment.currentWeapon.Attack(graphic.forward, currentTargets, attackLayerMask);
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
			foreach (var target in currentTargets)
			{
				unarmedDamage.source = data.characterId;
				target.TakeDamage(unarmedDamage, transform.position);
			}
		}
		
		#endregion

		#region Interaction

		protected void Interact()
		{
			if (currentInteraction != null)
			{
				if (currentInteraction.enabled) currentInteraction.transform.SendMessage("OnInteract", this);
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
				other.transform.SendMessage("OnCanInteract", SendMessageOptions.DontRequireReceiver);
			}
		}

		private void InteractionTriggerExit(Collider other)
		{
			if (other == currentInteraction)
			{
				other.transform.SendMessage("OnEndInteract", SendMessageOptions.DontRequireReceiver);
				currentInteraction = null;
			}
		}
		#endregion

		public bool SetHealth(float health)
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
			collider.enabled = false;
			data.isDead = true;
			DeathAnimationStarted();
			if(healthDisplay != null) healthDisplay.gameObject.SetActive(false);
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
					if (ammo == null || item.Equipped) ammo = item;
				}
			}

			if (ammo != null && !ammo.Equipped) ammo.Equipped = true;
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