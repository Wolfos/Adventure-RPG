using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Schema;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utility;
using WolfRPG.Core;

namespace Data
{
	public class SaveGameManager : MonoBehaviour
	{
		private Dictionary<string, SaveableObject> _activeSaveableObjects;
		private Dictionary<string, string> _saveData;

		public static bool NewGame = true;
		public static float TimeSinceLoad;
		private static float _lastLoadTime;
		
		public bool IsLoading { get; set; }
		
		private void Awake()
		{
			_lastLoadTime = Time.time;
			if(SceneManager.sceneCount == 1) SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
			SystemContainer.Register(this);
			_activeSaveableObjects = new();
			_saveData = new();
			NewGame = true;
			
			#if UNITY_EDITOR
			RPGDatabase.DefaultDatabase = null; // Force a reload
			#endif
		}

		private void Update()
		{
			TimeSinceLoad = Time.time - _lastLoadTime;
		}

		public void Register(SaveableObject saveableObject)
		{
			_activeSaveableObjects.Add(saveableObject.id, saveableObject);
		}
		
		public void Unregister(SaveableObject saveableObject)
		{
			_activeSaveableObjects.Remove(saveableObject.id);
			
			if (_saveData.ContainsKey(saveableObject.id)) _saveData.Remove(saveableObject.id);
			_saveData.Add(saveableObject.id, saveableObject.Save());
		}

		public void Save()
		{
			// Save all currently active objects
			foreach (var o in _activeSaveableObjects)
			{
				if (_saveData.ContainsKey(o.Key)) _saveData.Remove(o.Key);
				_saveData.Add(o.Key, o.Value.Save());
			}

			var allData = "";
			foreach (var data in _saveData)
			{
				allData += data.Key + ";";
				allData += data.Value + ";\n";
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

		public string GetData(SaveableObject saveableObject)
		{
			if (_saveData.ContainsKey(saveableObject.id))
			{
				return _saveData[saveableObject.id];
			}

			return string.Empty;
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
			CharacterPool.Clear();
			_activeSaveableObjects.Clear();
			AsyncOperation async;
			
			// Unload scenes
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				if(SceneManager.GetSceneAt(i).buildIndex == 0) continue;
				SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
			}
			
			//Load scenes
			async = SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);
			while (!async.isDone) yield return null;

			SceneManager.SetActiveScene(SceneManager.GetSceneByName("MainMenu"));
		}

		// New game
		private IEnumerator LoadGameRoutine()
		{
			RPGDatabase.DefaultDatabase = null; // Force a reload
			
			CharacterPool.Clear();
			AsyncOperation async;

			// Unload scenes
			for (int i = 0; i < SceneManager.sceneCount; i++)
			{
				if(SceneManager.GetSceneAt(i).buildIndex == 0) continue;
				SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i));
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
			RPGDatabase.DefaultDatabase = null; // Force a reload
			
			IsLoading = true;
			CharacterPool.Clear();
			NewGame = false;
			
			// Remove all non-global objects from the list. They will be destroyed
			var toRemove = (
				from obj in _activeSaveableObjects 
				where !obj.Value.global 
				select obj.Key).ToList();

			foreach (var key in toRemove)
			{
				_activeSaveableObjects.Remove(key);
			}


			yield return StartCoroutine(LoadGameRoutine());

			_lastLoadTime = Time.time;
			yield return null; // SaveableObjects will re-register themselves during this frame
			
			var allData = File.ReadAllText(Application.persistentDataPath + "/Save.json");
			allData = allData.Replace("\n", "");
			var data = allData.Split(';');

			_saveData.Clear();
			for(int i = 0; i < data.Length - 1; i++)
			{
				_saveData.Add(data[i], data[++i]);
			}

			foreach (var (key, value) in _activeSaveableObjects)
			{
				try
				{
					value.Load(_saveData[key]);
				}
				catch(KeyNotFoundException)
				{
					Debug.LogWarning($"Data for {key} was not found");
				}
			}
			
			IsLoading = false;
		}
	}
}