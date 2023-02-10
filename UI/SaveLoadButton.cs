using System.Collections;
using System.Collections.Generic;
using System.IO;
using Data;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
	public class SaveLoadButton : MonoBehaviour
	{
		[SerializeField] private bool selectedByDefault = false;
		[SerializeField] private bool isLoadButton;

		private bool lastControllerValue;

		private void OnEnable()
		{
			if(selectedByDefault && InputMapper.UsingController) GetComponent<Button>().Select();
			if (isLoadButton && !File.Exists(Application.persistentDataPath + "/Save.json"))
			{
				GetComponent<Button>().interactable = false;
			}
		}

		private void Update()
		{
			if(!lastControllerValue && InputMapper.UsingController && selectedByDefault) GetComponent<Button>().Select();
			lastControllerValue = InputMapper.UsingController;
		}

		public void LoadMainMenu()
		{
			Time.timeScale = 1;
			SystemContainer.GetSystem<SaveGameManager>().LoadMainMenu();
		}

		public void Save()
		{
			SystemContainer.GetSystem<SaveGameManager>().Save();
		}

		public void NewGame()
		{
			Debug.Log("Starting new game");
			Time.timeScale = 1;
			SystemContainer.GetSystem<SaveGameManager>().LoadGame();
		}

		public void Load()
		{
			Debug.Log("Loading saved game");
			Time.timeScale = 1;
			SystemContainer.GetSystem<SaveGameManager>().LoadSaveGame();
		}
	}
}