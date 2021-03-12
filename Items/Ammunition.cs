using System.Collections;
using System.Collections.Generic;
using Combat;
using Character;
using UnityEngine;

namespace Items
{
	public class Ammunition : Item
	{
		[SerializeField] private List<Damage> damage;
		private bool wasFired = false;
		private CharacterBase firedBy;

		private void OnEnable()
		{
			CheckCollider();
			CheckRigidbody();
			if (wasFired)
			{
				quantity = 1;
				GetComponent<Collider>().enabled = true;
				gameObject.layer = 0;
			}
		}

		private void OnTriggerEnter(Collider other)
		{
			if (!wasFired) return;
			
			var character = other.GetComponent<CharacterBase>();
			if (character == firedBy) return;
			
			// Hit character
			if (character != null)
			{
				foreach (var d in damage)
				{
					character.TakeDamage(d, transform.position);
					var rigidbody = GetComponent<Rigidbody>();
					rigidbody.isKinematic = true;
					transform.Translate(Vector3.down * 0.2f);
					transform.parent = character.graphic;
					wasFired = false;
					GetComponent<Collider>().enabled = false;
				}
			}
		}

		private void OnCollisionEnter(Collision other)
		{
			if (!wasFired) return;

			gameObject.layer = 11; // Interactable layer
			
			if (!other.gameObject.CompareTag("NoArrowPenetration"))
			{
				rigidbody.isKinematic = true;
				transform.Translate(Vector3.down * 0.2f);
				if (other.gameObject.layer != 11) // Interactable layer
				{
					transform.parent = other.transform;
				}
			}
			else
			{
				rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
				rigidbody.useGravity = true;
			}

			wasFired = false;
		}

		public void Fire(Vector3 direction, float force, Damage weaponDamage, CharacterBase firedBy)
		{
			GameObject go = Instantiate(gameObject);
			
			var ammo = go.GetComponent<Ammunition>();
			ammo.damage.Add(weaponDamage);
			ammo.wasFired = true;
			ammo.firedBy = firedBy;
			
			go.SetActive(true);
			go.transform.position = transform.parent.position + direction;
			go.transform.up = -direction;
			var rigidbody = go.GetComponent<Rigidbody>();
			rigidbody.isKinematic = false;
			rigidbody.useGravity = true;
			rigidbody.AddForce(direction * force);
			
			
			quantity--;
			if(quantity <= 0) container.DestroyItem(slot);
		}
	}
}