using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;

namespace Data
{
	public class SaveGameManager : MonoBehaviour
	{
		private Dictionary<string, SaveableObject> saveableObjects;
		private List<string> saveData;
		
		private void Awake()
		{
			if(SceneManager.sceneCount == 1) SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
			SystemContainer.Register(this);
			saveableObjects = new Dictionary<string, SaveableObject>();
		}

		public void Register(SaveableObject objectToRegister)
		{
			saveableObjects.Add(objectToRegister.id, objectToRegister);
		}

		public void Save()
		{
			string allData = "";
			foreach (var o in saveableObjects)
			{
				allData += o.Value.id + ";";
				allData += o.Value.Save() + ";\n";
			}

			if (Debug.isDebugBuild)
			{
				Debug.Log("Saving file " + Application.persistentDataPath + "/Save.json");
			}
			
			using (var file = new StreamWriter(Application.persistentDataPath + "/Save.json"))
			{
				file.Write(allData);
			}
		}

		public void LoadSaveGame()
		{
			StartCoroutine(LoadSaveRoutine());
		}

		public void LoadGame()
		{
			StartCoroutine(LoadGameRoutine());
		}

		public void LoadMainMenu()
		{
			StartCoroutine(LoadMainMenuRoutine());
		}

		private IEnumerator LoadMainMenuRoutine()
		{
			saveableObjects.Clear();
			AsyncOperation async;
			
			// Unload scenes
			if (SceneManager.GetSceneByName("MainMenu").isLoaded)
			{
				async = SceneManager.UnloadSceneAsync("MainMenu");
				while (!async.isDone) yield return null;
			}

			if (SceneManager.GetSceneByName("Game").isLoaded)
			{
				async = SceneManager.UnloadSceneAsync("Game");
				while (!async.isDone) yield return null;
			}

			if (SceneManager.GetSceneByName("GameUI").isLoaded)
			{
				async = SceneManager.UnloadSceneAsync("GameUI");
				while (!async.isDone) yield return null;
			}
			
			//Load scenes
			async = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
			while (!async.isDone) yield return null;

			SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
		}

		private IEnumerator LoadGameRoutine()
		{
			AsyncOperation async;
			
			// Unload scenes
			if (SceneManager.GetSceneByName("MainMenu").isLoaded)
			{
				async = SceneManager.UnloadSceneAsync("MainMenu");
				while (!async.isDone) yield return null;
			}

			if (SceneManager.GetSceneByName("Game").isLoaded)
			{
				async = SceneManager.UnloadSceneAsync("Game");
				while (!async.isDone) yield return null;
			}

			if (SceneManager.GetSceneByName("GameUI").isLoaded)
			{
				async = SceneManager.UnloadSceneAsync("GameUI");
				while (!async.isDone) yield return null;
			}

			//Load scenes
			async = SceneManager.LoadSceneAsync("GameUI", LoadSceneMode.Additive);
			while (!async.isDone) yield return null;
			async = SceneManager.LoadSceneAsync("Game", LoadSceneMode.Additive);
			while (!async.isDone) yield return null;
			
			SceneManager.SetActiveScene(SceneManager.GetSceneByName("Game"));
		}

		private IEnumerator LoadSaveRoutine()
		{
			saveableObjects.Clear();

			yield return StartCoroutine(LoadGameRoutine());
			
			yield return null; // SaveableObjects will re-register themselves during this frame
			
			string allData = File.ReadAllText(Application.persistentDataPath + "/Save.json");
			allData = allData.Replace("\n", "");
			string[] data = allData.Split(';');

			for(int i = 0; i < data.Length; i++)
			{
				if (!string.IsNullOrEmpty(data[i]))
				{
					try
					{
						saveableObjects[data[i]].Load(data[++i]);
					}
					catch (KeyNotFoundException e)
					{
						Debug.LogWarning("Savegame data did not find object: " + data[i]);
					}
				}
			}

			yield return null;
		}
	}
}