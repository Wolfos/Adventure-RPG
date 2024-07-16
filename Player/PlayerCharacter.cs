using System;
using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Character;
using Interface;
using OpenWorld;
using UI;
using Unity.Mathematics;
using UnityEngine.AI;
using Utility;
using WolfRPG.Core;
using WolfRPG.Core.CommandConsole;
using WolfRPG.Core.Quests;
using WolfRPG.Core.Statistics;
using World;
using Attribute = WolfRPG.Core.Statistics.Attribute;
using ItemContainer = Items.ItemContainer;

namespace Player
{
	public class TeleportCommand : IConsoleCommand
	{
		public string Word => "teleport";
		public ConsoleArgumentType[] Arguments { get; } = { ConsoleArgumentType.String };
		public void Execute(object[] arguments, Action<string> onError)
		{
			var name = (string)arguments[0];
			var destination = TeleportDestinations.GetDestination(name);
			PlayerCharacter.Teleport(destination);
		}
	}
	
	public class PlayerCharacter : CharacterBase
	{	
		[Header("PlayerCharacter")]
		[HideInInspector] public CharacterController characterController;
		
		[SerializeField] public AnimationCurve dodgeSpeed;
		[SerializeField] private new Transform camera;
		[SerializeField] private float interactionDistance = 1.0f;
		[SerializeField] private float offset = 1.09f;

		private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
		private static readonly int SkipAttackAnticipation = Animator.StringToHash("SkipAttackAnticipation");
		
		private static PlayerCharacter _instance;

		public Dictionary<string, QuestProgress> QuestProgress { get; set; }= new();
		
		[SerializeField, ObjectReference((int)DatabaseCategory.Global)] protected RPGObjectReference levelingSystemConfigObject;
		private LevelingSystemConfiguration _levelSystemConfig;

		[SerializeField] private PlayerControls playerControls;
		[SerializeField] private NavMeshObstacle navmeshObstacle;
		[SerializeField] private Transform playerCamera;

		private Vector3 _animatorStartPosition;
		

		private new void Awake()
		{
			_instance = this;
			
			base.Initialize();
			CharacterPool.RegisterPlayer(this);

			if (SaveGameManager.NewGame)
			{
				OnFinishedLoading();
			}
			animator.SetBool(SkipAttackAnticipation, true);
			_levelSystemConfig = levelingSystemConfigObject.GetComponent<LevelingSystemConfiguration>();
			//Data.OnXpAdded += OnSkillXpAdded;
			_animatorStartPosition = animator.transform.localPosition;
		}

		private new void Update()
		{
			if (WorldStreamer.IsReady == false)
			{
				return;
			}
			base.Update();

			Interaction();
			SprintXpGain();
		}

		private void LateUpdate()
		{
			var rootMotion = animator.GetBool("RootMotion");
			animator.applyRootMotion = rootMotion;
			 // if (rootMotion)
			 // {
				 var transform1 = animator.transform;
				 //var difference = transform1.localPosition - _animatorStartPosition;
				 transform1.localPosition = _animatorStartPosition;
				 transform1.localRotation = quaternion.identity;
				 //transform.Translate(transform1.TransformDirection(difference), Space.World);
				// Debug.Log(animator.velocity);
			//}
		}

		private void SprintXpGain()
		{
			if (IsSprinting)
			{
				//Data.AddSkillXp(Skill.Athletics, _levelSystemConfig.GetXpGainPerAction(Skill.Athletics) * Time.deltaTime);
			}
		}

		public void AddDodgeXP()
		{
			//Data.AddSkillXp(Skill.Athletics, _levelSystemConfig.GetXpGainPerAction(Skill.Athletics));

		}

		private void Interaction()
		{
			if (WindowManager.IsAnyWindowOpen()) return;
			
				// Subtract camera zoom
			var castDistance = interactionDistance - camera.localPosition.z;
			if (Physics.Raycast(camera.position, camera.forward, out var hit, castDistance, interactionLayerMask))
			{
				var other = hit.collider;
				if (CurrentInteraction == other)
				{
					InteractionUpdate(other);
				}
				else
				{
					EndInteraction(CurrentInteraction);
					InteractionStart(other);
				}
			}
			else
			{
				EndInteraction(CurrentInteraction);
			}
		}

		// TODO: Refactor into event
		public void OnFinishedLoading()
		{
			//Data.Attributes.OnAttributeUpdated += OnAttributeUpdated;
		}


		private new void Start()
		{
			CommandConsole.RegisterCommand(new TeleportCommand());
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			characterController = GetComponent<CharacterController>();

			base.Start();

			StartCoroutine(SetInitialHealth());
			
			currentWorldSpace = WorldStreamer.CurrentWorldSpace;
		}
		

		public static Vector3 GetPosition()
		{
			return _instance.transform.position;
		}

		public static ItemContainer GetInventory()
		{
			return _instance.Inventory;
		}
		
		public static CharacterEquipment GetEquipment()
		{
			return _instance.equipment;
		}

		public static CharacterDataObject GetData()
		{
			return _instance.dataObject;
		}

		private IEnumerator SetInitialHealth()
		{
			yield return null;
			//OnAttributeUpdated(Attribute.Health, Data.Attributes.Health);
			// TODO: Figure out this shit
		}

		private new void OnDestroy()
		{
			//Data.Attributes.OnAttributeUpdated -= OnAttributeUpdated;
			
			base.OnDestroy();
		}

		public override void StartQuest(string guid)
		{
			if (HasQuest(guid)) return;
			
			QuestProgress.Add(guid, new()
			{
				Guid = guid
			});
		}

		public static List<QuestProgress> GetAllQuestProgress()
		{
			var list = new List<QuestProgress>();
			foreach (var qp in _instance.QuestProgress)
			{
				list.Add(qp.Value);
			}

			return list;
		}

		public override QuestProgress GetQuestProgress(string guid)
		{
			return QuestProgress[guid];
		}

		public override bool HasQuest(string guid)
		{
			return QuestProgress.ContainsKey(guid);
		}

		private void OnAttributeUpdated(Attribute attribute, float newValue)
		{
			switch (attribute)
			{
				case Attribute.Health:
				{
					//EventManager.OnPlayerHealthChanged?.Invoke((int)newValue, Data.GetAttributeValue(Attribute.MaxHealth));
					if (newValue <= 0)
					{
						Die();
					}

					break;
				}
				case Attribute.Stamina:
				{
					//EventManager.OnPlayerStaminaChanged?.Invoke(newValue, Data.GetAttributeValue(Attribute.MaxStamina));
					break;
				}
			}
		}

		public static void Teleport(TeleportDestinations.Destination destination)
		{
			_instance.StartCoroutine(_instance.TeleportRoutine(destination));
		}

		private IEnumerator TeleportRoutine(TeleportDestinations.Destination destination)
		{
			PlayerControls.SetInputActive(false);
			LoadingScreen.StartLoading();
			while (LoadingScreen.IsDone == false)
			{
				yield return null;
			}
			
			EndInteraction(CurrentInteraction);

			if (destination.WorldSpace != currentWorldSpace)
			{
				if (destination.WorldSpace != WorldSpace.World)
				{
					CameraSettings.ToggleOcclusionCulling(false);
					WorldStreamer.EnterDungeon(destination.WorldSpace);
				}
				else
				{
					CameraSettings.ToggleOcclusionCulling(true);
					WorldStreamer.ExitDungeon(destination.Transform.position);
				}
			}
			
			
			characterController.enabled = false;

			while (WorldStreamer.IsReady == false)
			{
				yield return null;
			}
			
			// TODO: raycast for proper position on ground
			transform.position = destination.Transform.position;
			playerCamera.position = destination.Transform.position;
			currentWorldSpace = destination.WorldSpace;
			characterController.enabled = true;

			// Bit of extra wait for things to figure themselves out
			yield return new WaitForSeconds(0.2f);
			
			LoadingScreen.EndLoading();
			while (LoadingScreen.IsDone == false)
			{
				yield return null;
			}
			PlayerControls.SetInputActive(true);

		}

		public override void TakeDamage(float damage, float knockback, Vector3 point, CharacterBase other, Vector3 hitDirection = new())
		{
			base.TakeDamage(damage, knockback, point, other, hitDirection);
			
			ScreenShake.Shake(0.2f);
			Rumble.SetMotorSpeeds(1.0f, 0, 0.2f);
		}


		public override bool SetHealth(float health)
		{
			EventManager.OnPlayerHealthChanged?.Invoke(health, GetAttributeValue(Attribute.MaxHealth));
			return base.SetHealth(health);
		}

		protected override void DeathAnimationStarted()
		{
			PlayerControls.SetInputActive(false);
		}

		protected override void DeathAnimationFinished()
		{
			PlayerControls.SetInputActive(true);
			SaveGameManager.LoadSaveGame();
		}

		protected override void StartPet()
		{
			characterController.enabled = false;
			playerControls.enabled = false;
			navmeshObstacle.enabled = false;
		}
		
		protected override void EndPet()
		{
			characterController.enabled = true;
			playerControls.enabled = true;
			navmeshObstacle.enabled = true;
			
			//Data.AddSkillXp(Skill.AnimalHandling, _levelSystemConfig.GetXpGainPerAction(Skill.AnimalHandling));
		}

		// Handles leveling up
		private void OnSkillXpAdded(Skill skill)
		{
			// var xp = Data.GetSkillXP(skill);
			// var level = Data.GetSkillValue(skill);
			// if (xp >= LevelingSystemConfiguration.GetXpRequiredForLevel(level))
			// {
			// 	Data.SetSkillXp(skill, 0);
			// 	Data.IncreaseSkill(skill, 1);
			// 	Data.AddProgressTowardsNextLevel(1);
			//
			// 	if (Data.GetProgressTowardsNextLevel() >= _levelSystemConfig.SkillLevelsRequiredForLevel)
			// 	{
			// 		Data.SetProgressTowardsNextLevel(0);
			// 		Data.AddLevel(1);
			// 		Debug.Log($"Level up, new level is {Data.GetLevel()}");
			// 		Celebrations.MajorLevelUp(Data.GetLevel());
			// 	}
			// 	
			// 	Celebrations.MinorLevelUp(skill, level + 1);
			// 	Debug.Log($"Level up {skill}, new level is {level + 1}");
			// }
		}

		public override void HitDamageTaker(IDamageTaker damageTaker)
		{
			if (Weapon == null) return;
			
			//Data.AddSkillXp(Weapon.weaponData.AssociatedSkill, _levelSystemConfig.GetXpGainPerAction(Weapon.weaponData.AssociatedSkill));
			Rumble.SetMotorSpeeds(0, 1.0f, 0.1f);
			//Debug.Log(Data.GetSkillXP(Weapon.weaponData.AssociatedSkill));
		}

		public override void DidBlock()
		{
			base.DidBlock();
			
			//Data.AddSkillXp(Skill.Defense, _levelSystemConfig.GetXpGainPerAction(Skill.Defense));
		}

		public override void Teleport(Vector3 position, Quaternion rotation)
		{
			playerControls.Teleport(position + Vector3.up * offset);
			graphic.rotation = rotation;
		}

		public override void Killed(string characterGuid)
		{
			foreach (var qp in QuestProgress)
			{
				var questProgress = qp.Value;
				var quest = questProgress.GetQuest();
				var stage = quest.Stages[questProgress.CurrentStage];
				
				if (stage.Type == QuestStageType.KillX &&
				    stage.TargetNPC.Guid == characterGuid)
				{
					questProgress.StageProgress++;
					if (questProgress.StageProgress >= stage.Number)
					{
						Quest.ProgressToNextStage(questProgress);
					}
				}
			}
		}

	}
}