using System;
using System.Collections;
using Combat;
using Data;
using Dialogue;
using UI;
using UnityEngine;
using UnityEngine.AI;
using Utility;
using WolfRPG.Character;
using WolfRPG.Core;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using Random = UnityEngine.Random;

namespace Character
{
	public class NPC : CharacterBase
	{
		[SerializeField] private NavMeshAgent agent;

		[SerializeField] private float meleeAttackRange = 1;
		[SerializeField] private float maxPursueDistance = 20;
		[SerializeField] private float startChaseDistance = 10;

		private const float WanderWalkSpeed = 1;
		private const float CombatWalkSpeed = 2.5f;
		
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

			if (NpcComponent.Dialogue != null)
			{
				var dialogueStarter = gameObject.AddComponent<DialogueStarter>();
				dialogueStarter.dialogueAsset = NpcComponent.Dialogue.GetAsset<DialogueNodeGraph>();
			}

			if (NpcComponent.IsShopKeeper)
			{
				ShopInventory = new();
				var shopComponent = NpcComponent.Shop.GetComponent<ShopComponent>();
				foreach (var shopItem in shopComponent.ShopInventory)
				{
					ShopInventory.AddItem(shopItem.Item.Guid, shopItem.Quantity);
					ShopInventory.Money = shopComponent.BarteringMoney;
				}
			
				ShopInventory.Money = shopComponent.BarteringMoney;
				ShopInventory.PriceList = shopComponent.PriceList.GetComponent<PriceList>();
			}
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
			
			base.Start();
		}

		public void UpdateData()
		{
			SetHealth(Data.GetAttributeValue(Attribute.Health));
			transform.position = Data.CharacterComponent.Position;
			transform.rotation = Data.CharacterComponent.Rotation;
			graphic.localRotation = Quaternion.identity;
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
			if (Data.CharacterComponent.IsDead) return;
			
			StopAllCoroutines();
			
			NpcComponent.CurrentRoutine = routine;
			switch (routine)
			{
				case NPCRoutine.Idle:
					StartCoroutine(IdleRoutine());
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

		private void OnDamaged(float damage, string source)
		{
			if (!string.IsNullOrEmpty(source))
			{
				CharacterComponent.CurrentTarget = source;
				if (NpcComponent.CurrentRoutine != NPCRoutine.Combat)
				{
					ActivateRoutine(NPCRoutine.Combat);
				}
			}
			
			if (GetAttributeValue(Attribute.Health) - damage <= 0) // Dying
			{
				OnDeath?.Invoke();
				CharacterPool.GetCharacter(source)?.Killed(characterObjectRef.Guid);
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

			_movementSpeed = CombatWalkSpeed;
			
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
			var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
			float duration = 0;
			if (clipInfo.Length > 0)
			{
				var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
				duration = clip.length;
			}

			yield return new WaitForSeconds(duration - 0.1f);
			Attack();
		}

		private IEnumerator IdleRoutine()
		{
			yield return null;
			yield return null;

			if (!agent.enabled) agent.enabled = true;
			agent.destination = transform.position;
			
			while (true)
			{
				yield return null;
			}
		}
		
		private IEnumerator WanderingRoutine(bool delayed = false, bool proceed = false)
		{
			if (delayed)
			{
				yield return null;
				yield return null;
			}

			if (!agent.enabled) agent.enabled = true;

			_movementSpeed = WanderWalkSpeed;
			
			while (true)
			{
				// Proceed means proceed from saved game
				if (proceed) agent.velocity = CharacterComponent.Velocity;
				else NpcComponent.Destination = Bounds.RandomPos();

				agent.destination = NpcComponent.Destination;

				if (proceed) yield return null;

				while (agent.hasPath)
				{
					if (NpcComponent.Demeanor == NPCDemeanor.Hostile)
					{
						var playerCharacter = CharacterPool.GetPlayer();
						if (Vector3.SqrMagnitude(transform.position - playerCharacter.transform.position) < startChaseDistance * startChaseDistance)
						{
							CharacterComponent.CurrentTarget = playerCharacter.CharacterComponent.CharacterId;
							ActivateRoutine(NPCRoutine.Combat);
						}
					}
					if (Mathf.Abs(agent.velocity.magnitude) < 0.01f) // Stuck
					{
						break;
					}
					yield return null;
				}

				// Wait for 0 to 30 seconds before finding a new position to walk to
				var idleTime = Random.Range(0, 30);
				yield return new WaitForSeconds(idleTime);

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