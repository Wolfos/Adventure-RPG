using System;
using System.Collections;
using System.Collections.Generic;
using Combat;
using Items;
using UI;
using UnityEngine;
using Utility;

namespace NPC
{
	[RequireComponent(typeof(Container))]
	public abstract class Character : MonoBehaviour
	{
		public Container inventory;
		public CharacterData data;
		public Transform graphic;
		[SerializeField] protected Animator animator;
		[SerializeField] protected Transform rightHand, leftHand;
		[SerializeField] protected RuntimeAnimatorController unarmed;
		[SerializeField] private Renderer[] renderers;
		[SerializeField] private float startHealth;
		[SerializeField] private float healthOffset = 80;
		[SerializeField] protected float headOffset;
		[SerializeField] private float deathAnimationLength;
		[SerializeField] private CollisionCallbacks interactionTrigger, meleeAttackTrigger;
		[SerializeField] private LayerMask interactionLayerMask, attackLayerMask;
		[SerializeField] private Damage unarmedDamage;
		[SerializeField] private Collider collider;

		protected Item rightHandEquipped, leftHandEquipped, twoHandEquipped;
		private bool replaceEquippedItem;
		private List<Character> currentTargets;
		private Collider currentInteraction;

		protected Weapon currentWeapon;
		
		private HealthDisplay healthDisplay;
		protected Action<Damage> onDamaged;

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

			currentTargets = new List<Character>();
		}

		protected void OnEnable()
		{
			inventory.onItemEquipped += ItemEquipped;
			inventory.onItemUnequipped += ItemUnequipped;
			
			SetHealth(startHealth);
			
			if(healthDisplay != null) healthDisplay.gameObject.SetActive(false);

			collider.enabled = true;
		}

		protected void OnDisable()
		{
			inventory.onItemEquipped -= ItemEquipped;
			inventory.onItemUnequipped -= ItemUnequipped;
		}

		protected void Update()
		{
			if (data.health < startHealth && data.health > 0)
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
			List<Character> toRemove = new List<Character>();
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
				var character = other.GetComponent<Character>();
				if(character != this) currentTargets.Add(character);
			}
		}

		private void TargetTriggerExit(Collider other)
		{
			if (((1 << other.gameObject.layer) & attackLayerMask) != 0)
			{
				var character = other.GetComponent<Character>();
				if (currentTargets.Contains(character))
				{
					currentTargets.Remove(character);
				}
			}
		}

		protected void Attack()
		{
			bool willAttack = false;
			if (currentWeapon)
			{
				currentWeapon.baseDamage.source = this;
				if (currentWeapon is RangedWeapon) (currentWeapon as RangedWeapon).ammunition = GetAmmo();
				willAttack = currentWeapon.Attack(graphic.forward, currentTargets);
			}
			else // Unarmed attack
			{
				foreach (var target in currentTargets)
				{
					unarmedDamage.source = this;
					target.TakeDamage(unarmedDamage, transform.position);
				}

				willAttack = true;
			}
			
			if(willAttack) animator.SetTrigger("Attack");
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

		public void CheckEquipment()
		{
			foreach (var item in inventory.items)
			{
				if(item != null && item.Equipped) ItemEquipped(item);
			}
		}

		public void SetHealth(float health)
		{
			data.health = health;

			if (health <= 0)
			{
				Die();
			}
		}

		public void Die()
		{
			StopAllCoroutines();
			collider.enabled = false;
			ResetColours();
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
			ResetColours();
		}

		public void TakeDamage(Damage damage, Vector3 point)
		{
			onDamaged?.Invoke(damage);
			
			var knockback = (transform.position - point).normalized * damage.knockback * 10;
			StartCoroutine(HitFlash());
			StartCoroutine(Knockback(knockback));
			
			SetHealth(data.health - damage.amount);
		}

		private void ResetColours()
		{
			foreach (Renderer r in renderers)
			{
				foreach (Material m in r.materials)
				{
					m.SetColor("_Color", Color.white);
				}
			}
		}

		private IEnumerator HitFlash()
		{
			List<Material> materials = new List<Material>();
			foreach (Renderer r in renderers)
			{
				foreach (Material m in r.materials)
				{
					materials.Add(m);
				}
			}

			for (float t = 0; t < 1; t += Time.deltaTime * 4)
			{
				foreach (Material m in materials)
				{
					m.SetColor("_Color", m.GetColor("_Color") * (1 + Mathf.Sin(t * 10) * 0.15f));
				}
				yield return null;
			}
			
			foreach (Material m in materials)
			{
				m.SetColor("_Color", Color.white);
			}
		}

		private IEnumerator Knockback(Vector3 direction)
		{
			for (float t = 0; t < 1; t += Time.deltaTime * 8)
			{
				transform.Translate(direction * Time.deltaTime, Space.World);
				yield return null;
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
		
		private void ItemEquipped(Item item)
		{
			var t = item.transform;
			
			switch (item.type)
			{
				case ItemType.Consumable:
					break;
				case ItemType.RangedWeapon:
					if (rightHandEquipped)
					{
						replaceEquippedItem = true;
						rightHandEquipped.Equipped = false;
					}

					if (leftHandEquipped)
					{
						replaceEquippedItem = true;
						leftHandEquipped.Equipped = false;
					}
					
					if (twoHandEquipped && twoHandEquipped != item)
					{
						replaceEquippedItem = true;
						twoHandEquipped.Equipped = false;
					}

					t.parent = leftHand;
					t.localPosition = Vector3.zero;
					t.localRotation = Quaternion.identity;

					animator.runtimeAnimatorController = item.animationSet;
					twoHandEquipped = item;
					currentWeapon = item as Weapon;
					break;
				case ItemType.MeleeWeapon:
					if (rightHandEquipped && rightHandEquipped != item)
					{
						replaceEquippedItem = true;
						rightHandEquipped.Equipped = false;
					}
					
					if (twoHandEquipped)
					{
						replaceEquippedItem = true;
						twoHandEquipped.Equipped = false;
					}
					
					t.parent = rightHand;
					t.localPosition = Vector3.zero;
					t.localRotation = Quaternion.identity;

					animator.runtimeAnimatorController = item.animationSet;
					rightHandEquipped = item;
					currentWeapon = item as Weapon;
					break;
				case ItemType.Ammunition:
					break;
			}
		}

		private void ItemUnequipped(Item item)
		{
			switch (item.type)
			{
				case ItemType.Consumable:
					break;
				case ItemType.RangedWeapon:
					if (!replaceEquippedItem)
					{
						animator.runtimeAnimatorController = unarmed;
						leftHandEquipped = null;
						rightHandEquipped = null;
						twoHandEquipped = null;
						currentWeapon = null;
					}

					break;
				case ItemType.MeleeWeapon:
					if (!replaceEquippedItem)
					{
						animator.runtimeAnimatorController = unarmed;
						rightHandEquipped = null;
						currentWeapon = null;
					}

					break;
				case ItemType.Ammunition:
					break;
			}

			replaceEquippedItem = false;
		}
	}
}