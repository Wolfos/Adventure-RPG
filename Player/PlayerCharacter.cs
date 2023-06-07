using System.Collections;
using Data;
using UnityEngine;
using Character;
using Utility;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Player
{
	public class PlayerCharacter : CharacterBase
	{	
		[HideInInspector] public CharacterController characterController;
		
		[SerializeField] public AnimationCurve dodgeSpeed;

		private static readonly int IsAttacking = Animator.StringToHash("IsAttacking");

		private new void Awake()
		{
			base.Awake();
			SystemContainer.Register(this);
			
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

		private IEnumerator SetInitialHealth()
		{
			yield return null;
			OnAttributeUpdated(Attribute.Health, Data.Attributes.Health);
		}

		private new void OnDestroy()
		{
			SystemContainer.UnRegister<PlayerCharacter>();
			Data.Attributes.OnAttributeUpdated -= OnAttributeUpdated;
			
			base.OnDestroy();
		}

		// public void StartQuest(Quest quest)
		// {
		// 	if (HasQuest(quest)) return;
		// 	
		// 	var q = Instantiate(quest);
		// 	q.name = quest.name;
		// 	data.quests.Add(q);
		// 	data.questProgress.Add(quest.progress);
		// }

		// public bool HasQuest(Quest quest)
		// {
		// 	return data.quests.Any(x => x.name == quest.name);
		// }

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
			SystemContainer.GetSystem<SaveGameManager>().LoadSaveGame();
		}

	}
}