using System;
using System.Collections.Generic;
using System.Linq;
using Data;
using UnityEngine;
using Utility;
using XNode;

namespace Dialogue
{
	public class GetQuestStageNode : Node
	{
		[SerializeField] private Quest quest;
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output()] public Node doesntHaveQuest;
		[Output(instancePortList = true)] public List<string> stages;

		private void OnValidate()
		{
			if (!quest) return;
			stages = new List<string>();
			foreach (var stage in quest.stages)
			{
				stages.Add(stage.description);
			}
		}

		public Node GetNextNode()
		{
			var player = SystemContainer.GetSystem<Player.Player>();
			if (player.HasQuest(quest))
			{
				var playerQuest = player.data.quests.First(q => q.name == quest.name);
				return GetOutputPort("stages " + playerQuest.progress.currentStage).Connection.node;
			}
			else
			{
				return GetOutputPort("doesntHaveQuest").Connection.node;
			}
		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}
	}
}