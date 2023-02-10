using System;
using System.Collections;
using System.Linq;
using Data;
using UI;
using UnityEngine;
using Character;
using Utility;

namespace Player
{
	public class PlayerCharacter : CharacterBase
	{	
		[HideInInspector] public CharacterController characterController;
		
		[SerializeField] public AnimationCurve dodgeSpeed;

		private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

		private void Awake()
		{
			base.Awake();
			SystemContainer.Register(this);
		}


		private void Start()
		{
			Cursor.visible = false;
			Cursor.lockState = CursorLockMode.Locked;
			characterController = GetComponent<CharacterController>();

			base.Start();
		}

		private void OnDestroy()
		{
			SystemContainer.UnRegister<PlayerCharacter>();
		}

		public void StartQuest(Quest quest)
		{
			if (HasQuest(quest)) return;
			
			var q = Instantiate(quest);
			q.name = quest.name;
			data.quests.Add(q);
			data.questProgress.Add(quest.progress);
		}

		public bool HasQuest(Quest quest)
		{
			return data.quests.Any(x => x.name == quest.name);
		}

		public override bool SetHealth(float health)
		{
			EventManager.OnPlayerHealthChanged?.Invoke(health, data.maxHealth);
			return base.SetHealth(health);
		}

		protected override void DeathAnimationStarted()
		{
			PlayerControls.SetInputActive(false);
		}

		protected override void DeathAnimationFinished()
		{
			PlayerControls.SetInputActive(true);
			SystemContainer.GetSystem<SaveGameManager>().LoadSaveGame();
		}

	}
}