using System;
using System.Collections;
using Combat;
using Data;
using UI;
using UnityEngine;
using UnityEngine.AI;
using Utility;
using WolfRPG.Character;
using WolfRPG.Core;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Character
{
	public class NPC : CharacterBase
	{
		[SerializeField] private NavMeshAgent agent;

		[SerializeField] private float meleeAttackRange = 1;
		[SerializeField] private float maxPursueDistance = 20;
		[SerializeField] private float startChaseDistance = 10;
		
		public Bounds Bounds { get; set; }
		public bool Respawn { get; set; }

		public ItemContainer ShopInventory { get; set; }

		private float _movementSpeed;

		public Action OnDeath;
		public NpcComponent NpcComponent => Data.NpcComponent;
		
		private static readonly int Speed = Animator.StringToHash("Speed");

		public new void Initialize(RPGObjectReference characterObjectReference)
		{
			base.Initialize(characterObjectReference);
			_movementSpeed = agent.speed;

			// if (string.IsNullOrEmpty(shopObject.Guid) == false)
			// {
			// 	ShopInventory = new();
			// 	var shopComponent = shopObject.GetComponent<ShopComponent>();
			// 	foreach (var guid in shopComponent.StartingInventory)
			// 	{
			// 		ShopInventory.AddItem(guid);
			// 	}
			//
			// 	ShopInventory.Money = shopComponent.BarteringMoney;
			// }
		}

		private new void OnEnable()
		{
			if(agent.enabled) Debug.LogError("NavmeshAgent was enabled by default. Due to a bug in Unity, it should start disabled");

			if (Respawn)
			{
				Debug.Log("Respawning");
				animator.SetTrigger("Spawn");
				StartCoroutine(SpawnAnimation());
			}
			else
			{
				ActivateRoutine(NpcComponent.DefaultRoutine, true);
			}

			onDamaged += OnDamaged;
			
			base.OnEnable();
		}

		private IEnumerator SpawnAnimation()
		{
			yield return new WaitForSeconds(1);
			
			ActivateRoutine(NpcComponent.DefaultRoutine, true);
		}

		private void Start()
		{
			var transform1 = transform;
			CharacterComponent.Position = transform1.position;
			CharacterComponent.Rotation = transform1.rotation;
			
			equipment.CheckEquipment();
			
			base.Start();
		}

		public void UpdateData()
		{
			SetHealth(Data.GetAttributeValue(Attribute.Health));
			transform.position = Data.CharacterComponent.Position;
			transform.rotation = Data.CharacterComponent.Rotation;
			ActivateRoutine(Data.NpcComponent.CurrentRoutine, true, true);
		}

		protected override void DeathAnimationStarted()
		{
			StopAllCoroutines();
			agent.enabled = false;
		}

		protected override void DeathAnimationFinished()
		{
			Respawn = true;
			//gameObject.SetActive(false);
		}

		public void OpenShop()
		{
			ShopMenuWindow.SetData(ShopInventory);
			WindowManager.Open<ShopMenuWindow>();
		}
		
		private void ActivateRoutine(NPCRoutine routine, bool delayed = false, bool proceed = false)
		{
			StopAllCoroutines();
			
			NpcComponent.CurrentRoutine = routine;
			switch (routine)
			{
				case NPCRoutine.Idle:
					break;
				case NPCRoutine.Wandering:
					StartCoroutine(WanderingRoutine(delayed, proceed));
					break;
				case NPCRoutine.Combat:
					StartCoroutine(CombatRoutine(delayed, proceed));
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(routine), routine, null);
			}
		}

		private void Update()
		{
			// TODO: Fix when hit recoil is not present on animator
			// if (IsInHitRecoil)
			// {
			// 	agent.speed = 0;
			// }
			//else
			//{
				agent.speed = _movementSpeed;
			//}
			
			animator.SetFloat(Speed, agent.velocity.magnitude);

			var transform1 = transform;
			CharacterComponent.Position = transform1.position;
			CharacterComponent.Rotation = transform1.rotation;
			CharacterComponent.Velocity = agent.velocity;

			base.Update();
		}

		private void OnDamaged(float damage)
		{
			// TODO: Get damage source
			// if (!string.IsNullOrEmpty(damage.source))
			// {
			// 	CharacterComponent.CurrentTarget = damage.source;
			// 	if (NpcComponent.CurrentRoutine != NPCRoutine.Combat)
			// 	{
			// 		ActivateRoutine(NPCRoutine.Combat);
			// 	}
			// }
			//
			if (GetAttributeValue(Attribute.Health) - damage <= 0) // Dying
			{
				OnDeath?.Invoke();
				//CharacterPool.GetCharacter(damage.source)?.Killed(gameObject.name);
			}
			
		}

		private CharacterBase GetTarget()
		{
			return CharacterPool.GetCharacter(CharacterComponent.CurrentTarget);
		}

		private IEnumerator CombatRoutine(bool delayed = false, bool proceed = false)
		{
			if (delayed)
			{
				yield return null;
				yield return null;
			}

			if (!agent.enabled) agent.enabled = true;
			
			while (true)
			{
				if (proceed) 
				{
					agent.velocity = CharacterComponent.Velocity;
					yield return null;
				}

				while (GetTarget() == null) yield return null;

				Vector3 targetPos = GetTarget().transform.position;
				Vector3 pos = transform.position;
				
				NpcComponent.Destination = targetPos;
				agent.destination = targetPos;
				
				float distance = Vector3.Distance(targetPos, pos);
				var targetDir = targetPos - pos;
				targetDir.y = 0;
				float angle = Vector3.Angle(targetDir, graphic.forward);

				if (distance > maxPursueDistance)
				{
					ActivateRoutine(NpcComponent.DefaultRoutine);
				}
				else if (distance < meleeAttackRange && Mathf.Abs(angle) < 10)
				{
					NpcComponent.Destination = transform.position;
					agent.destination = NpcComponent.Destination;
					StartCoroutine(AttackRoutine());
					yield return new WaitForSeconds(1);
				}

				proceed = false;
				
				yield return null;
			}
		}

		private IEnumerator AttackRoutine()
		{
			animator.SetTrigger("Telegraph");
			yield return null;
			var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
			var length = clip.length;
			yield return new WaitForSeconds(length - 0.1f);
			Attack();
		}
		
		private IEnumerator WanderingRoutine(bool delayed = false, bool proceed = false)
		{
			if (delayed)
			{
				yield return null;
				yield return null;
			}

			if (!agent.enabled) agent.enabled = true;
			
			while (true)
			{
				if (proceed) agent.velocity = CharacterComponent.Velocity;
				else NpcComponent.Destination = Bounds.RandomPos();

				agent.destination = NpcComponent.Destination;

				if (proceed) yield return null;

				while (agent.hasPath)
				{
					if (NpcComponent.Demeanor == NPCDemeanor.Hostile)
					{
						// if (Vector3.SqrMagnitude(transform.position - _playerCharacter.transform.position) < startChaseDistance * startChaseDistance)
						// {
						// 	CharacterComponent.CurrentTarget = _playerCharacter.CharacterComponent.CurrentTarget;
						// 	ActivateRoutine(NPCRoutine.Combat);
						// }
					}
					if (Mathf.Abs(agent.velocity.magnitude) < 0.01f) // Stuck
					{
						break;
					}
					yield return null;
				}

				proceed = false;
				
				yield return null;
			}
		}

		private void OnDisable()
		{
			onDamaged -= OnDamaged;
		}
	}
}