using System.Collections;
using System.Collections.Generic;
using Data;
using UnityEngine;
using Character;
using UI;
using UnityEngine.AI;
using Utility;
using WolfRPG.Character;
using WolfRPG.Core;
using WolfRPG.Core.Quests;
using WolfRPG.Core.Statistics;
using WolfRPG.Inventory;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Player
{
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

		private new void Awake()
		{
			_instance = this;
			
			base.Initialize(characterObjectRef);
			CharacterPool.RegisterPlayer(this);

			if (SaveGameManager.NewGame)
			{
				OnFinishedLoading();
			}
			animator.SetBool(SkipAttackAnticipation, true);
			_levelSystemConfig = levelingSystemConfigObject.GetComponent<LevelingSystemConfiguration>();
			Data.OnXpAdded += OnSkillXpAdded;
		}

		private new void Update()
		{
			base.Update();

			Interaction();
			SprintXpGain();
		}

		private void SprintXpGain()
		{
			if (IsSprinting)
			{
				Data.AddSkillXp(Skill.Athletics, _levelSystemConfig.GetXpGainPerAction(Skill.Athletics) * Time.deltaTime);
			}
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
			Data.Attributes.OnAttributeUpdated += OnAttributeUpdated;
		}


		private new void Start()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			characterController = GetComponent<CharacterController>();

			base.Start();

			StartCoroutine(SetInitialHealth());
		}
		

		public static Vector3 GetPosition()
		{
			return _instance.transform.position;
		}

		public static ItemContainer GetInventory()
		{
			return _instance.Inventory;
		}

		public static CharacterData GetData()
		{
			return _instance.Data;
		}

		private IEnumerator SetInitialHealth()
		{
			yield return null;
			OnAttributeUpdated(Attribute.Health, Data.Attributes.Health);
		}

		private new void OnDestroy()
		{
			Data.Attributes.OnAttributeUpdated -= OnAttributeUpdated;
			
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
					EventManager.OnPlayerHealthChanged?.Invoke((int)newValue, Data.GetAttributeValue(Attribute.MaxHealth));
					if (newValue <= 0)
					{
						Die();
					}

					break;
				}
				case Attribute.Stamina:
				{
					EventManager.OnPlayerStaminaChanged?.Invoke(newValue, Data.GetAttributeValue(Attribute.MaxStamina));
					break;
				}
			}
		}

		public override bool SetHealth(int health)
		{
			EventManager.OnPlayerHealthChanged?.Invoke(health, Data.GetAttributeValue(Attribute.MaxHealth));
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
			
			Data.AddSkillXp(Skill.AnimalHandling, _levelSystemConfig.GetXpGainPerAction(Skill.AnimalHandling));
		}

		// Handles leveling up
		private void OnSkillXpAdded(Skill skill)
		{
			var xp = Data.GetSkillXP(skill);
			var level = Data.GetSkillValue(skill);
			if (xp >= LevelingSystemConfiguration.GetXpRequiredForLevel(level))
			{
				Data.SetSkillXp(skill, 0);
				Data.IncreaseSkill(skill, 1);
				Data.AddProgressTowardsNextLevel(1);

				if (Data.GetProgressTowardsNextLevel() >= _levelSystemConfig.SkillLevelsRequiredForLevel)
				{
					Data.SetProgressTowardsNextLevel(0);
					Data.AddLevel(1);
					Debug.Log($"Level up, new level is {Data.GetLevel()}");
					Celebrations.MajorLevelUp(Data.GetLevel());
				}
				
				Celebrations.MinorLevelUp(skill, level + 1);
				Debug.Log($"Level up {skill}, new level is {level + 1}");
			}
		}

		public override void HitEnemy(CharacterBase enemy)
		{
			if (Weapon == null) return;
			
			Data.AddSkillXp(Weapon.AssociatedSkill, _levelSystemConfig.GetXpGainPerAction(Weapon.AssociatedSkill));
			Debug.Log(Data.GetSkillXP(Weapon.AssociatedSkill));
		}

		public override void DidBlock()
		{
			base.DidBlock();
			
			Data.AddSkillXp(Skill.Defense, _levelSystemConfig.GetXpGainPerAction(Skill.Defense));
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