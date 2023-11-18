using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using Character;
using UI;
using Utility;
using WolfRPG.Core.Quests;
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

		private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");
		private static readonly int SkipAttackAnticipation = Animator.StringToHash("SkipAttackAnticipation");
		
		private static PlayerCharacter _instance;
		

		public Dictionary<string, QuestProgress> QuestProgress { get; set; }= new();


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
		}

		private new void Update()
		{
			base.Update();

			Interaction();
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

		private void OnAttributeUpdated(Attribute attribute, int newValue)
		{
			if (attribute == Attribute.Health)
			{
				EventManager.OnPlayerHealthChanged?.Invoke(newValue, Data.GetAttributeValue(Attribute.MaxHealth));
				if (newValue <= 0)
				{
					Die();
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
		
		public override void Killed(string characterGuid)
		{
			foreach (var qp in QuestProgress)
			{
				var questProgress = qp.Value;
				var quest = questProgress.GetQuest();
				var stage = quest.Stages[questProgress.CurrentStage];
				
				if (stage.Type == QuestStageType.KillX &&
				    stage.Target.Guid == characterGuid)
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