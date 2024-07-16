using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using Interface;
using UnityEngine;
using Utility;

namespace Items
{
	public class MeleeWeapon : Weapon
	{
		[Header("Melee weapon")]
		[SerializeField] private ParticleSystem attackFx;
		[SerializeField] private BoxCollider hitCollider;
		[SerializeField] private Collider blockCollider;

		private List<Matrix4x4> _cubeMatrices = new();
		private ParticleSystem _hitParticles;

		private void Start()
		{
			//_hitParticles = Instantiate(_weaponData.HitParticles.GetAsset<GameObject>()).GetComponent<ParticleSystem>(); // TODO: More performant to use only one of these throughout the game
		}

		public override void Attack(Vector3 direction, LayerMask attackLayerMask, LayerMask blockLayerMask, Action onStagger)
		{
			base.Attack(direction, attackLayerMask, blockLayerMask, onStagger);

			StartCoroutine(AttackRoutine(onStagger));
		}

		public override void StartBlock()
		{
			blockCollider.gameObject.SetActive(true);
		}
		
		public override void EndBlock()
		{
			blockCollider.gameObject.SetActive(false);
		}

		public override bool CanAttack()
		{
			return true;
		}

		private IEnumerator AttackRoutine(Action onStagger)
		{
			if (attackFx != null)
			{
				attackFx.Play();
			}

			Attacking = true;

			var previousPosition = transform.TransformPoint(hitCollider.center);
			var previousRotation = transform.rotation;
			var alreadyHit = new List<Rigidbody>();
			for (float t = 0; t < weaponData.AttackDuration; t += Time.deltaTime)
			{
				yield return new WaitForFixedUpdate();
				
				if(alreadyHit.Count > 0) continue;
				
				// Do a sweep to interpolate between animation frames
				var currentPosition = transform.TransformPoint(hitCollider.center);
				var currentRotation = transform.rotation;
				var distance = Vector3.Distance(previousPosition, currentPosition);
				var amount = distance / (hitCollider.size.x / 2);
				
				// Colliders we hit, position we hit them at
				var hits = new List<Tuple<Collider[], Vector3>>();
				for (var i = 0; i < amount; i++)
				{
					var center = Vector3.Lerp(previousPosition, currentPosition, (float) i / amount);
					var rotation = Quaternion.Lerp(previousRotation, currentRotation, (float) i / amount);

					var blockResult = Physics.OverlapBox(center, hitCollider.size, rotation, BlockLayerMask);
					if (blockResult.Length > 0 && blockResult[0] != blockCollider) // This works because if it's not at 0, there's multiple hits
					{
						foreach (var blockCollider in blockResult)
						{
							var otherWeapon = blockCollider.GetComponentInParent<Weapon>();
							otherWeapon.Blocked();
						}
						onStagger?.Invoke();
						goto EndAttack;
					}
						
					var result = Physics.OverlapBox(center, hitCollider.size, rotation, AttackLayerMask);
					
					// Gizmos
					var matrix = Matrix4x4.TRS(center, rotation, hitCollider.size);
					_cubeMatrices.Add(matrix);
					
					hits.Add(new (result, center));
				}
				
				// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
				// Check each collider if it's an enemy and not already hit during this attack
				foreach (var tuple in hits)
				{
					var colliders = tuple.Item1;
					var hitPosition = tuple.Item2;
					
					foreach (var collider in colliders)
					{
						if (collider.attachedRigidbody == null) continue;
						if (alreadyHit.Contains(collider.attachedRigidbody)) continue;
						alreadyHit.Add(collider.attachedRigidbody);

						var damageTaker = collider.attachedRigidbody.GetComponent<IDamageTaker>();
						if (damageTaker == null) continue;
						if (damageTaker.CanTakeDamage() == false) continue;
						if (damageTaker is CharacterBase @base && @base == Character) continue;
						AttackHit(damageTaker, collider.ClosestPoint(hitPosition), currentPosition - previousPosition);
					}
				}
				
				previousPosition = currentPosition;
				previousRotation = currentRotation;
			}
			
			EndAttack:
			if (attackFx != null)
			{
				attackFx.Stop();
			}


			Attacking = false;
		}

		private void OnDrawGizmos()
		{
			foreach (var matrix in _cubeMatrices)
			{
				Gizmos.matrix = matrix;
				Gizmos.color = Color.red;
				Gizmos.DrawCube(Vector3.zero, Vector3.one);
			}
			
			_cubeMatrices.Clear();
		}

		protected override void OnEquipped(Item item)
		{
			base.OnEquipped(item);
			Rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
			Rigidbody.isKinematic = true;
		}
		
		protected override void OnUnEquipped(Item item)
		{
			base.OnUnEquipped(item);
			Rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
		}

		private void AttackHit(IDamageTaker damageTaker, Vector3 hitPosition, Vector3 hitDirection)
		{
			SFXPlayer.PlaySound(weaponData.HitSound);
			if (damageTaker != null)
			{
				//var position = transform.position;
				damageTaker.TakeDamage(weaponData.BaseDamage, weaponData.Knockback, hitPosition, Character, hitDirection);
				Character.HitDamageTaker(damageTaker);
				// if (_hitParticles != null)
				// {
				// 	_hitParticles.transform.position = hitPosition;
				// 	_hitParticles.Play();
				// }
			}
		}
	}
}
