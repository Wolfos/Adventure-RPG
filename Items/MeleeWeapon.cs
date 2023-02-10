﻿using System;
using System.Collections;
using System.Collections.Generic;
using Character;
using UnityEngine;
using Utility;

namespace Items
{
	public class MeleeWeapon : Weapon
	{
		[Header("Melee weapon")]
		[SerializeField] private TrailRenderer attackFx;
		[SerializeField] private float attackDuration = 1;
		[SerializeField] private ParticleSystem hitParticles;
		[SerializeField] private BoxCollider hitCollider;
		[SerializeField] private GameObject blockCollider;

		private List<Matrix4x4> _cubeMatrices = new();

		public override void Attack(Vector3 direction, LayerMask attackLayerMask, Action onStagger)
		{
			base.Attack(direction, attackLayerMask, onStagger);

			StartCoroutine(AttackRoutine(onStagger));
		}

		public override void StartBlock()
		{
			blockCollider.SetActive(true);
		}
		
		public override void EndBlock()
		{
			blockCollider.SetActive(false);
		}

		private IEnumerator AttackRoutine(Action onStagger)
		{
			attackFx.emitting = true;
			Attacking = true;

			var previousPosition = transform.TransformPoint(hitCollider.center);
			var previousRotation = transform.rotation;
			var alreadyHit = new List<Rigidbody>();
			for (float t = 0; t < attackDuration; t += Time.deltaTime)
			{
				yield return new WaitForFixedUpdate();
				
				// Do a sweep to interpolate between animation frames
				var currentPosition = transform.TransformPoint(hitCollider.center);
				var currentRotation = transform.rotation;
				var distance = Vector3.Distance(previousPosition, currentPosition);
				var amount = distance / (hitCollider.size.x / 2);
				var colliders = new List<Collider>();
				for (var i = 0; i < amount; i++)
				{
					var center = Vector3.Lerp(previousPosition, currentPosition, (float) i / amount);
					var rotation = Quaternion.Lerp(previousRotation, currentRotation, (float) i / amount);

					var blockResult = Physics.OverlapBox(center, hitCollider.size, rotation, blockLayerMask);
					if (blockResult.Length > 0)
					{
						onStagger?.Invoke();
						goto EndAttack;
					}
						
					var result = Physics.OverlapBox(center, hitCollider.size, rotation, AttackLayerMask);
					
					// Gizmos
					var matrix = Matrix4x4.TRS(center, rotation, hitCollider.size);
					_cubeMatrices.Add(matrix);
					
					colliders.AddRange(result);
				}
				
				// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
				// Check each collider if it's an enemy and not already hit during this attack
				foreach (var collider in colliders)
				{
					if (collider.attachedRigidbody == null) continue;
					if(alreadyHit.Contains(collider.attachedRigidbody)) continue;
					alreadyHit.Add(collider.attachedRigidbody);
					
					var otherCharacter = collider.attachedRigidbody.GetComponent<CharacterBase>();
					if (otherCharacter == Character) continue;
					if (otherCharacter == null) continue;
					AttackHit(otherCharacter, transform.position);
				}
				
				previousPosition = currentPosition;
				previousRotation = currentRotation;
			}
			
			EndAttack:
			attackFx.emitting = false;
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

		private void AttackHit(CharacterBase otherCharacter, Vector3 hitPosition)
		{
			if (otherCharacter != null && otherCharacter != Character)
			{
				//var position = transform.position;
				otherCharacter.TakeDamage(baseDamage, hitPosition);
				hitParticles.transform.position = hitPosition;
				hitParticles?.Play();
			}
		}
	}
}
