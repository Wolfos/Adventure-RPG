using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Data
 {
	 [Serializable]
	 public class QuestProgress
	 {
		 public int id;
		 public bool complete;
		 public int currentStage;

		 public Quest GetQuest()
		 {
			 var prefab = Database.GetDatabase<QuestDatabase>().quests[id];
			 var quest = Object.Instantiate(prefab);
			 quest.name = prefab.name;
			 quest.progress = this;
			 return quest;
		 }
	 }
	 
	 [CreateAssetMenu(fileName = "Data", menuName = "eeStudio/Quest", order = 1)]
	 public class Quest : ScriptableObject
	 {
		 public string questName;
		 public QuestStage[] stages;
		 [HideInInspector]public QuestProgress progress = new QuestProgress();
		 [HideInInspector] public int id;
		 

		 public QuestStage stage => stages[progress.currentStage];

		 public void Progress()
		 {
			 if (stage.type == QuestStageType.Finished) return;
			 
			 Debug.Log("Quest progress");
			 stage.progress++;
			 if (stage.progress >= stage.number)
			 {
				 stage.complete = true;
				 Debug.Log("Completed quest stage");
				 progress.currentStage++;
				 if (stage.type == QuestStageType.Finished) progress.complete = true;
			 }
		 }

		 public void SetStage(int newStage, bool failed = false)
		 {
			 stage.complete = !failed;
			 progress.currentStage = newStage;
			 if (stage.type == QuestStageType.Finished)
			 {
				 progress.complete = true;
				 stage.complete = true;
			 }
		 }
	 }

	 public enum QuestStageType
	 {
		 KillX, FindObject, SpeakTo, Finished
	 }

	 [Serializable]
	 public class QuestStage
	 {
		 public QuestStageType type;
		 public string target;
		 public int number;
		 public string description;
		 [HideInInspector] public int progress;
		 [HideInInspector] public bool complete;
	 }
 }