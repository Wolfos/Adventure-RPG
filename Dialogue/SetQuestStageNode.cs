using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character;
using Data;
using UnityEngine;
using Utility;
using WolfRPG.Core;
using WolfRPG.Core.Quests;
using XNode;

namespace Dialogue
{
	public class SetQuestStageNode : Node
	{
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output] public Node next;
		[SerializeField, ObjectReference(5)] private RPGObjectReference questReference;
		[SerializeField] private int stage;

		private void OnValidate()
		{
			var quest = Quest.GetQuest(questReference.Guid);
			stage = Mathf.Clamp(stage, 0, quest.Stages.Length - 1);
		}

		public void Execute(CharacterBase character)
		{
			if (character.HasQuest(questReference.Guid))
			{
				Quest.SetStage(character.GetQuestProgress(questReference.Guid), stage);
			}
		}
	}
}