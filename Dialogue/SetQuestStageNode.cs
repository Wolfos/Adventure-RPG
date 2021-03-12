using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using Utility;
using XNode;

namespace Dialogue
{
	public class SetQuestStageNode : Node
	{
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output] public Node next;
		[SerializeField] private Quest quest;
		[SerializeField] private int stage;

		private void OnValidate()
		{
			stage = Mathf.Clamp(stage, 0, quest.stages.Length - 1);
		}

		public void Execute()
		{
			var player = SystemContainer.GetSystem<Player.Player>();
			if (player.HasQuest(quest))
			{
				var playerQuest = player.data.quests.First(q => q.name == quest.name);
				playerQuest.SetStage(stage);
			}
		}
	}
}