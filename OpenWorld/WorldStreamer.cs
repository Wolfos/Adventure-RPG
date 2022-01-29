#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Experimental.SceneManagement;
#endif

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenWorld
{
    [ExecuteInEditMode]
    public class WorldStreamer : MonoBehaviour
    {
        [SerializeField] private OpenWorldManager data;
        private readonly List<Chunk> _currentChunks = new List<Chunk>();
        private bool _bakingMode;
        private bool _dungeonMode;
        private static WorldStreamer _instance;

        private void Start()
        {
            _instance = this;
#if UNITY_EDITOR
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.OpenScene($"Assets/Scenes/Terrains.unity", OpenSceneMode.Additive);
                return;
            }
#endif
            SceneManager.LoadScene("Terrains", LoadSceneMode.Additive);
        }

#if UNITY_EDITOR
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

        private void UpdateSelectionChunks()
        {
            selectedObject = Selection.activeTransform;
            if (selectedObject != null &&
                selectedObject.parent == null &&
                selectedObject.gameObject.scene.name.Contains("Chunk"))
            {
                var oldScene = selectedObject.gameObject.scene;
                var chunk = data.GetChunkByPosition(selectedObject.position.x, selectedObject.position.z);
                chunk?.AddObject(selectedObject.gameObject);
                if (chunk.scene != oldScene)
                {
                    EditorSceneManager.MarkSceneDirty(chunk.scene);
                    EditorSceneManager.MarkSceneDirty(oldScene);
                }
            }
        }

        private void SelectPrevious()
        {
            if (selectedObject != null)
            {
                Selection.activeTransform = selectedObject;
            }
        }

        [MenuItem("Game/Toggle baking mode")]
        public static void ToggleBakingMode()
        {
            if (EditorApplication.isPlaying) return;

            _instance._bakingMode = !_instance._bakingMode;
            if (_instance._bakingMode)
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
#endif

        public static void EnterDungeon(string sceneName)
        {
            _instance.StartCoroutine(_instance.EnterDungeonCoroutine(sceneName));
        }

        private IEnumerator EnterDungeonCoroutine(string sceneName)
        {
            var sceneLoad = SceneManager.LoadSceneAsync(sceneName);
            _dungeonMode = true;
            while (sceneLoad.isDone == false)
            {
                // TODO: Show loading screen
                yield return null;
            }
            
            foreach (var chunk in _currentChunks)
            {
                SceneManager.UnloadSceneAsync(chunk.Name);
            }

            _currentChunks.Clear();
        }

        public static void ExitDungeon()
        {
            _instance._dungeonMode = false;
        }
        
        private void Update()
        {
            if (_bakingMode || _dungeonMode) return;
            
            var mainCamera = Camera.main;
            #if UNITY_EDITOR
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
            {
                return;
            }
            if (!EditorApplication.isPlaying)
            {
                mainCamera = SceneView.lastActiveSceneView.camera;
                if (UnloadUnnecessaryScenes())
                {
                    SelectPrevious();
                }
                UpdateSelectionChunks();
            }
            #endif
            var cameraPos = mainCamera.transform.position;
            var chunks = data.GetChunkAndAdjacent(cameraPos.x, cameraPos.z);
            var toRemove = new List<Chunk>();
            foreach (var chunk in _currentChunks)
            {
                if (!chunks.Contains(chunk))
                {
                    toRemove.Add(chunk);
                    #if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                    {
                        if (!chunk.scene.isDirty)
                        {
                            EditorSceneManager.CloseScene(chunk.scene, true);
                            SelectPrevious();
                        }
                        continue;
                    }
                    #endif
                    SceneManager.UnloadSceneAsync(chunk.Name);
                }
            }

            foreach (var chunk in toRemove)
            {
                _currentChunks.Remove(chunk);
            }
            
            foreach (var chunk in chunks)
            {
                if (!_currentChunks.Contains(chunk))
                {
                    _currentChunks.Add(chunk);
                    #if UNITY_EDITOR
                    if (!EditorApplication.isPlaying)
                    {
                        EditorSceneManager.OpenScene($"Assets/Scenes/Chunks/{chunk.Name}.unity", OpenSceneMode.Additive);
                        SelectPrevious();
                        continue;
                    }
                    #endif
                    SceneManager.LoadSceneAsync(chunk.Name, LoadSceneMode.Additive);
                    
                }
            }
            
        }
        
        private void OnDrawGizmos()
        {
            // Draws a blue line from this transform to the target
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(transform.position, new Vector3(500, 0, 500));
            int minX = -(data.chunksX / 2) * data.chunkWidth;
            int minZ = -(data.chunksZ / 2) * data.chunkDepth;
            for (int x = 0; x < data.chunksX; x++)
            {
                for (int z = 0; z < data.chunksZ; z++)
                {
                    var chunkMin = new Vector3(minX + x * data.chunkWidth, 0, minZ + z * data.chunkDepth);
                    var chunkMax = new Vector3(chunkMin.x + data.chunkWidth, 0, chunkMin.z + data.chunkDepth);

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
        }
    }
}