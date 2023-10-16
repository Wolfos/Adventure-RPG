using System.Collections.Generic;
using Character;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using WolfRPG.Core;
using WolfRPG.Core.Quests;
using XNode;

namespace Dialogue
{
	public class GetQuestStageNode : Node
	{
		[FormerlySerializedAs("quest")] [SerializeField, ObjectReference(5)] private RPGObjectReference questReference;
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output()] public Node doesntHaveQuest;
		[Output(instancePortList = true)] public List<string> stages;
		[Output()] public Node finishedQuest;

		private void OnValidate()
		{
			if (questReference == null) return;
			var quest = questReference.GetComponent<QuestData>();
			stages = new();
			foreach (var stage in quest.Stages)
			{
				stages.Add(stage.Description);
			}
		}

		public Node GetNextNode(CharacterBase character)
		{
			if (character.HasQuest(questReference.Guid))
			{
				var questProgress = character.GetQuestProgress(questReference.Guid);
				if (questProgress.Complete)
				{
					return GetOutputPort("finishedQuest").Connection.node;
				}
				return GetOutputPort("stages " + questProgress.CurrentStage).Connection.node;
			}

			return GetOutputPort("doesntHaveQuest").Connection.node;
		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}
	}
}