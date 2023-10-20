using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using WolfRPG.Core;

namespace Data
{
	public class SaveGameManager : MonoBehaviour
	{
		private static Dictionary<string, ISaveData> _saveData;

		public static bool NewGame = true;
		public static float TimeSinceLoad;
		private static float _lastLoadTime;
		
		public static bool IsLoading { get; set; }

		private static SaveGameManager _instance;
		public static Action OnSave;
		
		private void Awake()
		{
			_instance = this;
			_lastLoadTime = Time.time;
			if(SceneManager.sceneCount == 1) SceneManager.LoadSceneAsync("MainMenu", LoadSceneMode.Additive);

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

		public static bool HasData(string id)
		{
			return _saveData.ContainsKey(id);
		}

		/// <summary>
		/// Register a new object
		/// </summary>
		public static void Register(string id, ISaveData saveData)
		{
			_saveData.Add(id, saveData);
		}

		public static void Save()
		{
			OnSave?.Invoke(); // Tell objects to update their save data. Not all objects do this though. Most read/write to their save data directly
			
			var json = JsonConvert.SerializeObject(_saveData, WolfRPG.Core.Settings.JsonSerializerSettings);

			if (Debug.isDebugBuild)
			{
				Debug.Log("Saving file " + Application.persistentDataPath + "/Save.json");
			}
			
			using (var file = new StreamWriter(Application.persistentDataPath + "/Save.json"))
			{
				file.Write(json);
			}
		}

		public static ISaveData GetData(string id)
		{
			if (_saveData.TryGetValue(id, out var data))
			{
				return data;
			}

			return null;
		}


		public static void LoadSaveGame()
		{
			_instance.StartCoroutine(nameof(LoadSaveRoutine));
		}

		public static void LoadGame()
		{
			_instance.StartCoroutine(nameof(LoadGameRoutine));
		}

		public static void LoadMainMenu()
		{
			_instance.StartCoroutine(nameof(LoadMainMenuRoutine));
		}

		private IEnumerator LoadMainMenuRoutine()
		{
			CharacterPool.Clear();
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
			
			var json = File.ReadAllText(Application.persistentDataPath + "/Save.json");
			_saveData = JsonConvert.DeserializeObject<Dictionary<string, ISaveData>>(json, WolfRPG.Core.Settings.JsonSerializerSettings);

			yield return StartCoroutine(LoadGameRoutine());

			_lastLoadTime = Time.time;

			yield return null; // SaveableObjects will re-register themselves during this frame

			IsLoading = false;
		}
	}
}