using System;
using System.Collections;
using AI;
using Data;
using Dialogue;
using Items;
using Sirenix.OdinInspector;
using UI;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using ItemContainer = Items.ItemContainer;
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
		[SerializeField] private new Collider collider; // TODO: replace with mainCollider?

		private const float WanderWalkSpeed = 1;
		private const float CombatWalkSpeed = 2.5f;

		private float _previousAngularVelocity;

		public Bounds boundaries;
		public bool Respawn { get; set; }

		public ItemContainer ShopInventory { get; set; }

		private float _movementSpeed;

		public Action OnDeath;
		public NPCDataObject NpcData { get; private set; }
		
		private static readonly int Speed = Animator.StringToHash("Speed");
		private static readonly int SidewaysSpeed = Animator.StringToHash("SidewaysSpeed");
		private static readonly int AlreadyDead = Animator.StringToHash("AlreadyDead");

		private NPCRoutine _preDialogueRoutine;

		public new void Initialize()
		{
			base.Initialize();
			NpcData = dataObject as NPCDataObject;
			if (NpcData is null)
			{
				Debug.LogError($"{dataObject.characterName.Get()} data object is not valid NPC data");
				return;
			}
			_movementSpeed = agent.speed;

			if (string.IsNullOrEmpty(NpcData.dialogueStartNode) == false)
			{
				var dialogueStarter = gameObject.AddComponent<DialogueStarter>();
				dialogueStarter.dialogueCharacter = this;
				dialogueStarter.startNode = NpcData.dialogueStartNode;
			}

			if (NpcData.isShopKeeper)
			{
				ShopInventory = new();
				var shop = NpcData.shop;
				foreach (var shopItem in shop.shopInventory)
				{
					ShopInventory.AddItem(shopItem.item.Guid, shopItem.quantity);
				}
			
				ShopInventory.Money = shop.barteringMoney;
				ShopInventory.PriceList = shop.priceList;
			}
		}

		private void Awake()
		{
			boundaries.center = transform.position;
			Initialize();
			
			if (Respawn) // TODO: Respawn probably broken, definitely not used atm
			{
				Debug.Log("Respawning");
				animator.SetTrigger("Spawn");
				StartCoroutine(SpawnAnimation());
			}
			else
			{
				ActivateRoutine(NpcData.defaultRoutine, true);
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
			
			ActivateRoutine(NpcData.defaultRoutine, true);
		}

		private void Start()
		{
			var transform1 = transform;
			SaveData.Position = transform1.position;
			SaveData.Rotation = transform1.rotation;
			
			base.Start();
		}

		public void Resume()
		{
			ActivateRoutine(SaveData.CurrentRoutine, true, true);
		}

		public void UpdateData()
		{
			// TODO: Attributes
			// if (Data.GetAttributeValue(Attribute.Health) <= 0)
			// {
			// 	animator.SetTrigger(AlreadyDead);
			// }
			//
			// SetHealth(Data.GetAttributeValue(Attribute.Health));
			// transform.position = Data.CharacterComponent.Position;
			// transform.rotation = Data.CharacterComponent.Rotation;
			// graphic.localRotation = Quaternion.identity;
			// ActivateRoutine(Data.NpcComponent.CurrentRoutine, true, true);
		}

		protected override void DeathAnimationStarted()
		{
			StopAllCoroutines();
			agent.enabled = false;
		}

		protected override void DeathAnimationFinished()
		{
			Respawn = true;

			var go = new GameObject("Lootable Corpse");
			go.transform.SetParent(transform);
			go.layer = gameObject.layer;
			var lootable = go.AddComponent<LootableCorpse>();
			lootable.Npc = this;
			lootable.trackTransform = middleSpine;
			var sphereCollider = go.AddComponent<SphereCollider>();
			sphereCollider.isTrigger = true;
			sphereCollider.radius = 0.5f;

			foreach (var c in GetComponents<Collider>())
			{
				c.enabled = false;
			}
			//gameObject.SetActive(false);
		}

		public void OpenShop()
		{
			ShopMenuWindow.SetData(ShopInventory);
			WindowManager.Open<ShopMenuWindow>();
		}
		
		private void ActivateRoutine(NPCRoutine routine, bool delayed = false, bool proceed = false)
		{
			if (SaveData.IsDead) return;
			
			StopAllCoroutines();
			
			SaveData.CurrentRoutine = routine;
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

		private void AIUpdate()
		{
			// If we're hostile, always be on the lookout for the player
			if (NpcData.demeanor == NPCDemeanor.Hostile)
			{
				var playerCharacter = CharacterPool.GetPlayer();
				if (Vector3.SqrMagnitude(transform.position - playerCharacter.transform.position) < startChaseDistance * startChaseDistance)
				{
					SaveData.CurrentTarget = playerCharacter.GetId();
					if (SaveData.CurrentRoutine != NPCRoutine.Combat)
					{
						ActivateRoutine(NPCRoutine.Combat);
					}
				}
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
			
			AIUpdate();

			var speed = agent.velocity.magnitude;
			if (math.abs(speed) < 0.12f) speed = 0;
			
			animator.SetFloat(Speed, speed);
			var transform1 = transform;

			var previousRotation = SaveData.Rotation.eulerAngles.y;
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

			
			SaveData.Position = transform1.position;
			SaveData.Rotation = transform1.rotation;
			SaveData.Velocity = agent.velocity;

			// Some functions rotate the "graphic" instead but since NavmeshAgent rotates this object, we'll want to apply that
			transform.rotation *= graphic.localRotation;
			graphic.localRotation = Quaternion.identity;

			base.Update();
		}

		private void OnDamaged(float damage, Guid source)
		{
			if (source != Guid.Empty)
			{
				SaveData.CurrentTarget = source;
				if (SaveData.CurrentRoutine != NPCRoutine.Combat)
				{
					ActivateRoutine(NPCRoutine.Combat);
				}
			}
			
			if (GetAttributeValue(Attribute.Health) - damage <= 0) // Dying
			{
				Destroy(GetComponent<DialogueStarter>());
				collider.enabled = false;
				OnDeath?.Invoke();
				CharacterPool.GetCharacter(source)?.Killed(SaveData.CharacterId.ToString());
			}
			
		}

		public void StartDialogue()
		{
			_preDialogueRoutine = SaveData.CurrentRoutine;
			ActivateRoutine(NPCRoutine.Idle);
		}

		public void StopDialogue()
		{
			ActivateRoutine(_preDialogueRoutine);
		}

		private CharacterBase GetTarget()
		{
			return CharacterPool.GetCharacter(SaveData.CurrentTarget);
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
					agent.velocity = SaveData.Velocity;
					yield return null;
				}

				while (GetTarget() == null) yield return null;

				Vector3 targetPos = GetTarget().transform.position;
				Vector3 pos = transform.position;
				
				SaveData.Destination = targetPos;
				agent.destination = targetPos;
				
				float distance = Vector3.Distance(targetPos, pos);
				var targetDir = targetPos - pos;
				targetDir.y = 0;
				float angle = Vector3.Angle(targetDir, graphic.forward);

				if (distance > maxPursueDistance)
				{
					ActivateRoutine(NpcData.defaultRoutine);
				}
				else if (distance < meleeAttackRange && Mathf.Abs(angle) < 30) // Try to attack
				{
					SaveData.Destination = transform.position;
					agent.destination = SaveData.Destination;
					yield return AttackRoutine();
				}
				else if (distance < meleeAttackRange)
				{
					SaveData.Destination = transform.position;
					agent.destination = SaveData.Destination;

					const float speed = 10;
					for (float t = 0; t < 1; t += Time.deltaTime * 4)
					{
						var newDirection = Vector3.RotateTowards(transform.forward, targetDir, speed * Time.deltaTime, 0.0f);
						transform.rotation = Quaternion.LookRotation(newDirection);
						yield return null;
					}
				}

				proceed = false;
				
				yield return null;
			}
		}

		private IEnumerator AttackRoutine()
		{
			//animator.SetTrigger("Telegraph"); // TODO: This is not a thing
			//yield return null;
			//var clipInfo = animator.GetCurrentAnimatorClipInfo(0);
			//float duration = 0;
			
			// if (clipInfo.Length > 0)
			// {
			// 	var clip = animator.GetCurrentAnimatorClipInfo(0)[0].clip;
			// 	duration = clip.length;
			// }
			Attack();

			yield return new WaitForSeconds(1);
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
				if (proceed) agent.velocity = SaveData.Velocity;
				else SaveData.Destination = boundaries.RandomPos();

				agent.destination = SaveData.Destination;

				if (proceed) yield return null;

				while (agent.hasPath)
				{
					
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
			ActivateRoutine(SaveData.CurrentRoutine);
		}

		private void OnDisable()
		{
			onDamaged -= OnDamaged;
		}

		[Button("Render")]
		private void RenderNPC()
		{
			var visualData = dataObject.visualData;

			partPicker.EnableHair();
			foreach (var item in dataObject.startingInventory)
			{
				var equipment = item.itemData as EquipmentData;
				if(equipment == null || equipment.EquipmentParts == null) continue;

				if (equipment.HideHair)
				{
					partPicker.DisableHair();
				}
				
				foreach (var part in equipment.EquipmentParts)
				{
					CharacterCustomizationController.SetPart(part.Part, ref visualData, part.Index);
					if (equipment.Material != 0)
					{
						visualData.MaterialOverrides.Add(part.Part, equipment.Material);
					}
				}
			}
			
			CharacterCustomizationController.ApplyNewVisualData(visualData, partPicker);

			gameObject.name = dataObject.characterName.Get(SystemLanguage.English);
		}
	}
}