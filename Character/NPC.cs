﻿using System;
using System.Collections;
using AI;
using Data;
using Dialogue;
using Sirenix.OdinInspector;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using WolfRPG.Character;
using WolfRPG.Core;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using Random = UnityEngine.Random;

namespace Character
{
	public class NPC : CharacterBase
	{
		[Header("NPC")]
		[SerializeField] private NavMeshAgent agent;

		[SerializeField] private float meleeAttackRange = 1;
		[SerializeField] private float maxPursueDistance = 20;
		[SerializeField] private float startChaseDistance = 10;
		[SerializeField] private Collider collider;

		private const float WanderWalkSpeed = 1;
		private const float CombatWalkSpeed = 2.5f;

		private float _previousAngularVelocity;

		public Bounds boundaries;
		public bool Respawn { get; set; }

		public ItemContainer ShopInventory { get; set; }

		private float _movementSpeed;

		public Action OnDeath;
		public NpcComponent NpcComponent => Data.NpcComponent;
		
		private static readonly int Speed = Animator.StringToHash("Speed");
		private static readonly int SidewaysSpeed = Animator.StringToHash("SidewaysSpeed");
		

		public new void Initialize(RPGObjectReference characterObjectReference)
		{
			base.Initialize(characterObjectReference);
			_movementSpeed = agent.speed;

			if (NpcComponent.Dialogue != null)
			{
				var dialogueStarter = gameObject.AddComponent<DialogueStarter>();
				dialogueStarter.dialogueCharacter = this;
				dialogueStarter.dialogueAsset = NpcComponent.Dialogue.GetAsset<DialogueNodeGraph>();
			}

			if (NpcComponent.IsShopKeeper)
			{
				ShopInventory = new();
				var shopComponent = NpcComponent.Shop.GetComponent<ShopComponent>();
				foreach (var shopItem in shopComponent.ShopInventory)
				{
					ShopInventory.AddItem(shopItem.Item.Guid, shopItem.Quantity);
				}
			
				ShopInventory.Money = shopComponent.BarteringMoney;
				ShopInventory.PriceList = shopComponent.PriceList.GetComponent<PriceList>();
			}
		}

		private void Awake()
		{
			boundaries.center = transform.position;
			Initialize(characterObjectRef);
			
			if (Respawn) // TODO: Respawn probably broken, definitely not used atm
			{
				Debug.Log("Respawning");
				animator.SetTrigger("Spawn");
				StartCoroutine(SpawnAnimation());
			}
			else
			{
				ActivateRoutine(NpcComponent.DefaultRoutine, true);
			}
			
			NPCManager.Register(this);
			
			if(agent.enabled) Debug.LogError("NavmeshAgent was enabled by default. Due to a bug in Unity, it should start disabled");
		}

		private new void OnEnable()
		{
			onDamaged += OnDamaged;
			
			//base.OnEnable();
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

		public void Resume()
		{
			ActivateRoutine(Data.NpcComponent.CurrentRoutine, true, true);
		}

		public void UpdateData()
		{
			if (Data.GetAttributeValue(Attribute.Health) <= 0)
			{
				animator.SetTrigger("AlreadyDead");
			}
			
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
				agent.speed = _movementSpeed * SpeedMultiplier;
			//}

			var speed = agent.velocity.magnitude;
			if (math.abs(speed) < 0.12f) speed = 0;
			
			animator.SetFloat(Speed, speed);
			var transform1 = transform;

			var previousRotation = CharacterComponent.Rotation.eulerAngles.y;
			var currentRotation = transform1.rotation.eulerAngles.y;
			var angularVelocity = (currentRotation - previousRotation) * Time.deltaTime;
			angularVelocity = Mathf.Lerp(_previousAngularVelocity, angularVelocity, 0.5f);

			if (math.abs(angularVelocity) > 0.05f)
			{
				animator.SetFloat(SidewaysSpeed, angularVelocity);
			}
			else
			{
				animator.SetFloat(SidewaysSpeed, 0);
			}
			_previousAngularVelocity = angularVelocity;

			
			CharacterComponent.Position = transform1.position;
			CharacterComponent.Rotation = transform1.rotation;
			CharacterComponent.Velocity = agent.velocity;

			// Some functions rotate the "graphic" instead but since NavmeshAgent rotates this object, we'll want to apply that
			transform.rotation *= graphic.localRotation;
			graphic.localRotation = Quaternion.identity;

			base.Update();
		}

		private void OnDamaged(float damage, Guid source)
		{
			if (source != Guid.Empty)
			{
				CharacterComponent.CurrentTarget = source;
				if (NpcComponent.CurrentRoutine != NPCRoutine.Combat)
				{
					ActivateRoutine(NPCRoutine.Combat);
				}
			}
			
			if (GetAttributeValue(Attribute.Health) - damage <= 0) // Dying
			{
				Destroy(GetComponent<DialogueStarter>());
				collider.enabled = false;
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
				else NpcComponent.Destination = boundaries.RandomPos();

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

		public void WalkToAndStop(Vector3 destination, float totalTime)
		{
			StopAllCoroutines();
			StartCoroutine(WalkToAndStopRoutine(destination, totalTime));
		}

		private IEnumerator WalkToAndStopRoutine(Vector3 destination, float totalTime)
		{
			agent.destination = destination;
			yield return new WaitForSeconds(totalTime);
			ActivateRoutine(NpcComponent.CurrentRoutine);
		}

		private void OnDisable()
		{
			onDamaged -= OnDamaged;
		}

		[Button("Render")]
		private void RenderNPC()
		{
			var characterComponent = characterObjectRef.GetComponent<CharacterComponent>().CreateInstance();
			var loadoutComponent = characterObjectRef.GetComponent<LoadoutComponent>();

			var visualData = characterComponent.VisualData;

			foreach (var item in loadoutComponent.StartingInventory)
			{
				var equipment = item.ItemObject.GetComponent<EquipmentData>();
				if(equipment.EquipmentParts == null) continue;
				
				foreach (var part in equipment.EquipmentParts)
				{
					CharacterCustomizationController.SetPart(part.Part, visualData, part.Index);
					if (equipment.Material != 0)
					{
						visualData.MaterialOverrides.Add(part.Part, equipment.Material);
					}
				}
			}
			
			CharacterCustomizationController.SetData(visualData, partPicker);

			gameObject.name = characterObjectRef.GetObject().Name;
		}
	}
}