#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using World;

namespace OpenWorld
{
    [ExecuteInEditMode]
    public class WorldStreamer : MonoBehaviour
    {
        public static Action OnWorldSpaceChanged;
        private static WorldSpace _currentWorldSpace;
        public static WorldSpace CurrentWorldSpace
        {
            get
            {
                return _currentWorldSpace;
            }
            private set
            {
                _currentWorldSpace = value;
                OnWorldSpaceChanged?.Invoke();
            }
        }

        
        [SerializeField] private OpenWorldManager data;
        private readonly List<Chunk> _currentChunks = new();
        private static bool _streamingDisabled;
        private static WorldStreamer _instance;
        private AsyncOperation _unloadAssetsOperation;
        private const int MaxUnloadBeforeCleanup = 20;
        private int _unloadedSceneCounter;
        private Camera _mainCamera;
        public static bool BakingMode { get; private set; }
        public static bool IsReady { get; private set; }
        

        private void Start()
        {
            _instance = this;
            _mainCamera = Camera.main;
            IsReady = true;

            if (CurrentWorldSpace == WorldSpace.World)
            {
                // Load terrain scene if not currently loaded
#if UNITY_EDITOR
                EditorApplication.wantsToQuit += OnQuit;
                if (!EditorApplication.isPlaying)
                {
                    foreach (var sceneSetup in EditorSceneManager.GetSceneManagerSetup())
                    {
                        if (sceneSetup.isLoaded && sceneSetup.path.Contains("Terrains.unity"))
                        {
                            return;
                        }
                    }

                    EditorSceneManager.OpenScene("Assets/Scenes/Terrains.unity", OpenSceneMode.Additive);
                    return;
                }
#endif
                if (SceneManager.GetSceneByName("Terrains").isLoaded) return;

                SceneManager.LoadScene("Terrains", LoadSceneMode.Additive);
            }
        }

#if UNITY_EDITOR
        private void OnDestroy()
        {
            EditorApplication.wantsToQuit -= OnQuit;
        }
        
        private bool UnloadUnnecessaryScenes()
        {
            bool sceneWasUnloaded = false;
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isDirty) continue;
                if (scene.name.Contains("Chunk"))
                {
                    if (_currentChunks.All(chunk => chunk.Name != scene.name))
                    {
                        EditorSceneManager.CloseScene(scene, true);
                        sceneWasUnloaded = true;
                    }
                }

            }

            return sceneWasUnloaded;
        }

        private Transform selectedObject;

        public static void PutSelectedObjectInChunk()
        {
            _instance.UpdateSelectionChunks(true);
        }

        private void UpdateSelectionChunks(bool force = false)
        {
            selectedObject = Selection.activeTransform;
            if (selectedObject != null &&
                (selectedObject.parent == null &&
                selectedObject.gameObject.scene.name.Contains("Chunk")) || force)
            {
                var oldScene = selectedObject.gameObject.scene;
                var chunk = data.GetChunkByWorldPosition(selectedObject.position.x, selectedObject.position.z);
                if (chunk == null || chunk.Name == string.Empty)
                {
                    var chunkPosition = data.WorldToChunkPosition(selectedObject.position.x, selectedObject.position.z);
                    chunk = data.AddChunk(chunkPosition.X, chunkPosition.Z);
                    return;
                }
                
                if (chunk.scene != oldScene)
                {
                    chunk?.AddObject(selectedObject.gameObject);
                    EditorSceneManager.MarkSceneDirty(chunk.scene);
                    EditorSceneManager.MarkSceneDirty(oldScene);
                }
            }
        }

        private void SelectPrevious()
        {
            // if (selectedObject != null)
            // {
            //     Selection.activeTransform = selectedObject;
            // }
        }

        [MenuItem("eeStudio/Toggle baking mode")]
        public static void ToggleBakingMode()
        {
            if (EditorApplication.isPlaying) return;

            BakingMode = !BakingMode;
            if (BakingMode)
            {
                foreach (var chunk in _instance.data.chunks)
                {
                    if (!_instance._currentChunks.Contains(chunk))
                    {
                        _instance._currentChunks.Add(chunk);

                        EditorSceneManager.OpenScene($"Assets/Scenes/Chunks/{chunk.Name}.unity",
                            OpenSceneMode.Additive);
                        _instance.SelectPrevious();
                    }
                }
            }
            else
            {
                _instance.UnloadUnnecessaryScenes();
            }
        }

        [MenuItem("eeStudio/Setup World Streamer")]
        public static void SetupWorldStreamer()
        {
            if (EditorApplication.isPlaying) return;

            if (_instance.data.chunks != null && _instance.data.chunks.Count > 0)
            {
                Debug.LogError("Data already has chunks, setup cancelled");
                return;
            }

            _instance.data.chunks = new();

            BakingMode = true;

            AssetDatabase.CreateFolder("Assets/Scenes", "Chunks");
            AssetDatabase.Refresh();

            var sceneCamera = SceneView.lastActiveSceneView.camera;
            var cameraPos = sceneCamera.transform.position;
            var chunkPosition = _instance.data.WorldToChunkPosition(cameraPos.x, cameraPos.z);
            _instance.data.AddChunk(chunkPosition.X, chunkPosition.Z);
            
            BakingMode = false;
        }
#endif

        public static void EnterDungeon(WorldSpace worldSpace)
        {
            CurrentWorldSpace = worldSpace;
            IsReady = false;

            var sceneName = WorldSpaces.GetSceneName(worldSpace);
            _instance.StartCoroutine(_instance.EnterDungeonCoroutine(sceneName));
        }

        #if UNITY_EDITOR
        public static void EnterWorldSpaceEditor(WorldSpace worldSpace, bool openScenes = true)
        {
            CurrentWorldSpace = worldSpace;
            
            if (worldSpace == WorldSpace.World)
            {
                if (openScenes)
                {
                    EditorSceneManager.OpenScene("Assets/Scenes/ManagerScene.unity");
                    EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Additive);
                    EditorSceneManager.OpenScene("Assets/Scenes/GameUI.unity", OpenSceneMode.Additive);
                }

                _streamingDisabled = false;
            }
            else
            {
                //
                if (openScenes)
                {
                    var sceneName = WorldSpaces.GetSceneName(worldSpace);
                    EditorSceneManager.OpenScene("Assets/Scenes/ManagerScene.unity");
                    EditorSceneManager.OpenScene("Assets/Scenes/Game.unity", OpenSceneMode.Additive);
                    EditorSceneManager.OpenScene("Assets/Scenes/GameUI.unity", OpenSceneMode.Additive);
                    EditorSceneManager.OpenScene($"Assets/Scenes/Dungeons/{sceneName}.unity", OpenSceneMode.Additive);
                }

                _streamingDisabled = true;
            }
        }
        #endif

        private IEnumerator EnterDungeonCoroutine(string sceneName)
        {
            var sceneLoad = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            sceneLoad.allowSceneActivation = false;
            _streamingDisabled = true;
            while (sceneLoad.progress < 0.9f) // Last 10% needs to be synchronous
            {
                yield return null;
            }

            sceneLoad.allowSceneActivation = true;
            foreach (var chunk in _currentChunks)
            {
                var chunkUnload = SceneManager.UnloadSceneAsync(chunk.Name);
                while (chunkUnload.progress < 0.9f)
                {
                    yield return null;
                }
            }

            var terrainUnload = SceneManager.UnloadSceneAsync("Terrains");
            while (terrainUnload.progress < 0.9f)
            {
                yield return null;
            }
            _currentChunks.Clear();

            
            IsReady = true;
        }

        public static void ExitDungeon(Vector3 exitPosition)
        {
            _instance.StartCoroutine(_instance.ExitDungeonRoutine(exitPosition));
        }

        private IEnumerator ExitDungeonRoutine(Vector3 exitPosition)
        {
            IsReady = false;
            var dungeon = WorldSpaces.GetSceneName(CurrentWorldSpace);
            var unload = SceneManager.UnloadSceneAsync(dungeon);
            while (unload.progress < 0.9f) // Last 10% needs to be synchronous
            {
                yield return null;
            }
            CurrentWorldSpace = WorldSpace.World;

            var load = SceneManager.LoadSceneAsync("Terrains", LoadSceneMode.Additive);
            while (load.progress < 0.9f) // Last 10% needs to be synchronous
            {
                yield return null;
            }

            yield return LoadChunksAtLocation(exitPosition);
            
            IsReady = true;
            _streamingDisabled = false;
        }

        private IEnumerator LoadChunksAtLocation(Vector3 location)
        {
            var chunks = data.GetChunkAndAdjacent(location.x, location.z);
            
            // Iterate in reverse for safe removal
            for (int i = _currentChunks.Count - 1; i >= 0; i--)
            {
                var chunk = _currentChunks[i];
                if (chunks.Contains(chunk) == false)
                {
                    _currentChunks.RemoveAt(i);

                    var async = SceneManager.UnloadSceneAsync(chunk.Name);
                    while (async.isDone ==false)
                    {
                        yield return null;
                    }
                    _unloadedSceneCounter++;
                }
            }
            
            foreach (var chunk in chunks)
            {
                if (_currentChunks.Contains(chunk) == false)
                {
                    _currentChunks.Add(chunk); 
                    var async = SceneManager.LoadSceneAsync(chunk.Name, LoadSceneMode.Additive);
                    while (async.isDone == false)
                    {
                        yield return null;
                    }
                }
            }
        }
        
        public static void ForceUpdate()
        {
            _instance.Update();
        }
        
        private void Update()
        {
            _instance = this;
            
            if (BakingMode || _streamingDisabled) return;

            var mainCamera = _mainCamera;
#if UNITY_EDITOR
            if (EditorApplication.isPlaying == false)
            {
                if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                {
                    return;
                }
                
                mainCamera = SceneView.lastActiveSceneView.camera;
                if (UnloadUnnecessaryScenes())
                {
                    SelectPrevious();
                }
                //UpdateSelectionChunks();
            }
#endif
            var cameraPos = mainCamera.transform.position;
            var chunks = data.GetChunkAndAdjacent(cameraPos.x, cameraPos.z);
            
            // Iterate in reverse for safe removal
            for (int i = _currentChunks.Count - 1; i >= 0; i--)
            {
                var chunk = _currentChunks[i];
                if (chunks.Contains(chunk) == false)
                {
                    _currentChunks.RemoveAt(i);
#if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                    {
                        if (chunk.scene.isDirty == false)
                        {
                            EditorSceneManager.CloseScene(chunk.scene, true);
                            SelectPrevious();
                        }
                        continue;
                    }
#endif
                    SceneManager.UnloadSceneAsync(chunk.Name);
                    _unloadedSceneCounter++;
                }
            }

            if (_unloadedSceneCounter >= MaxUnloadBeforeCleanup && (_unloadAssetsOperation == null || _unloadAssetsOperation.isDone))
            {
                Debug.Log("Unloading unused assets");
                _unloadAssetsOperation = Resources.UnloadUnusedAssets();
                _unloadedSceneCounter = 0;
            }
            
            foreach (var chunk in chunks)
            {
                if (_currentChunks.Contains(chunk) == false)
                {
                    #if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                    {
                        var sceneIsLoaded = false;
                        for (var i = 0; i < EditorSceneManager.sceneCount; i++)
                        {
                            var scene = EditorSceneManager.GetSceneAt(i);
                            if (scene.name == chunk.Name)
                            {
                                sceneIsLoaded = true;
                            }
                        }
            
                        if (!sceneIsLoaded)
                        {
                            EditorSceneManager.OpenScene($"Assets/Scenes/Chunks/{chunk.Name}.unity", OpenSceneMode.Additive);
                            _currentChunks.Add(chunk);
                        }
                        SelectPrevious();
                        continue;
                    }
                    else
                    {
                        _currentChunks.Add(chunk);
                        SceneManager.LoadSceneAsync(chunk.Name, LoadSceneMode.Additive);
                    }
                    #else
                    _currentChunks.Add(chunk);
                    SceneManager.LoadSceneAsync(chunk.Name, LoadSceneMode.Additive);
                    #endif
                }
            }
        }
        
        #if UNITY_EDITOR
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            // Recompilation resets static vars, so we check active scenes to set the worldspace correctly after recompilation
            for (var i = 0; i < Enum.GetNames(typeof(WorldSpace)).Length; i++)
            {
                var worldSpace = (WorldSpace) i;
                var sceneName = WorldSpaces.GetSceneName(worldSpace);
                var scene = EditorSceneManager.GetSceneByName(sceneName);
                if (scene.IsValid())
                {
                    EnterWorldSpaceEditor(worldSpace, false);
                    break;
                }
            }
        }
        #endif
        
        private void OnDrawGizmos()
        {
            // Draws a blue line from this transform to the target
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(transform.position, new Vector3(500, 0, 500));
            
            foreach (var chunk in data.chunks)
            {
                var chunkMin = chunk.min;
                var chunkMax = chunk.max;

                if (chunk.scene.isLoaded)
                {
                    Gizmos.color = Color.yellow;
                }
                else
                {
                    Gizmos.color = Color.red;
                }
                
                var topLeft = new Vector3(chunkMin.x, 25, chunkMin.z);
                var bottomLeft = new Vector3(chunkMin.x, 25, chunkMax.z);
                var topRight = new Vector3(chunkMax.x, 25, chunkMin.z);
                var bottomRight = new Vector3(chunkMax.x, 25, chunkMax.z);
                    
                Gizmos.DrawLine(topLeft, bottomLeft);
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(topRight, topLeft);
            }
        }

        private bool OnQuit()
        {
            _streamingDisabled = true;
            return true;
        }
    }
}