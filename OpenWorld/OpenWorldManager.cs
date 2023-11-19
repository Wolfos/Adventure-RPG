using System;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using System.IO;
//using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace OpenWorld
{
    [CreateAssetMenu(menuName = "eeStudio/Open World Manager")]
    public class OpenWorldManager : ScriptableObject
    {
        public int chunkWidth, chunkDepth;
        public int chunksX, chunksZ;
        public List<Chunk> chunks;

        public Object worldScene;
        
        // Chunks sorted by position. Contains empty chunk when there's no chunk at said position
        private Chunk[] _sortedChunks;

        public struct ChunkPosition
        {
            public ChunkPosition(int x, int z)
            {
                X = x;
                Z = z;
            }
            public int X, Z;
        }

        public ChunkPosition WorldToChunkPosition(float x, float z)
        {
            var chunkX = Mathf.FloorToInt(x / chunkWidth) +(chunksX / 2);
            var chunkZ = Mathf.FloorToInt(z / chunkDepth) +(chunksZ / 2);

            return new ChunkPosition(chunkX, chunkZ);
        }

        public Chunk GetChunkByWorldPosition(float x, float z)
        {
            var chunkPosition = WorldToChunkPosition(x, z);

            return GetChunkByChunkPosition(chunkPosition.X, chunkPosition.Z);
        }

        private void SortChunks()
        {
            // Get total width / depth
            foreach (var chunk in chunks)
            {
                if (chunk.x > chunksX) chunksX = chunk.x;
                if (chunk.z > chunksZ) chunksZ = chunk.z;
            }
            
            _sortedChunks = new Chunk[chunksX * chunksZ];

            foreach (var chunk in chunks)
            {
                _sortedChunks[chunk.z + chunk.x * chunksX] = chunk;
            }
        }
        
        private Chunk GetChunkByChunkPosition(int x, int z)
        {
            if(_sortedChunks == null || _sortedChunks.Length == 0) SortChunks();
            if (z + x * chunksX < 0 || z + x * chunksX > _sortedChunks.Length) return null;
            return _sortedChunks[z + x * chunksX];
        }

        public List<Chunk> GetChunkAndAdjacent(float x, float z)
        {
            var ret = new List<Chunk>();
            int chunkX = Mathf.FloorToInt(x / chunkWidth) +(chunksX / 2);
            int chunkZ = Mathf.FloorToInt(z / chunkDepth) +(chunksZ / 2);

            void AddIfNotNullOrEmpty(Chunk chunk)
            {
                if(chunk != null && chunk.Name != string.Empty) ret.Add(chunk);
            }
            
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX, chunkZ));
            
            // Directly adjacent
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX-1, chunkZ-1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX, chunkZ-1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX-1, chunkZ));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX+1, chunkZ));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX+1, chunkZ+1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX, chunkZ+1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX-1, chunkZ+1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX+1, chunkZ-1));
            
            // Distance of two chunks
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX - 2, chunkZ -1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX - 2, chunkZ));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX - 2, chunkZ + 1));
            
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX - 1, chunkZ - 2));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX, chunkZ - 2));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX + 1, chunkZ - 2));
            
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX + 2, chunkZ - 1));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX + 2, chunkZ));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX + 2, chunkZ + 1));
            
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX - 1, chunkZ + 2));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX, chunkZ + 2));
            AddIfNotNullOrEmpty(GetChunkByChunkPosition(chunkX + 1, chunkZ + 2));
            
            return ret;
        }
        
#if UNITY_EDITOR
        //[Button("Generate Chunks")]
        [Obsolete("Chunks are now created dynamically when needed")]
        public void GenerateChunks()
        {
            // TODO: Maybe make it work on additional runs?
            //if (chunks != null) return;

            if (chunks == null || chunks.Count == 0)
            {
                chunks = new List<Chunk>();

                for (int x = 0; x < chunksX; x++)
                {
                    for (int z = 0; z < chunksZ; z++)
                    {
                        chunks.Add(CreateNewChunk("Chunk", x, z));
                    }
                }
            }

            var scenePath = AssetDatabase.GetAssetPath(worldScene);
            var scene = SceneManager.GetSceneByPath(scenePath);

            //var terrainChunk = CreateNewChunk("Terrains", 0, 0);
            var dirtyChunks = new List<Chunk>();
            foreach (var obj in scene.GetRootGameObjects())
            {
                if (obj.GetComponent<Terrain>() != null)
                {
                    continue;
                }

                if (obj.CompareTag("Dungeon"))
                {
                    continue;
                }

                
                foreach (Transform child in obj.transform)
                {
                    //var chunk = chunks.FirstOrDefault(c => c.FitsInChunk(child));
                    var chunk = GetChunkByWorldPosition(child.position.x, child.position.z);
                    if (chunk == null)
                    {
                        Debug.LogError($"{child.name} was outside of scene");
                    }
                    else
                    {
                        chunk.AddObject(child.gameObject);
                        if(!dirtyChunks.Contains(chunk)) dirtyChunks.Add(chunk);
                    }
                }
            }
            
            //chunks.Add(terrainChunk);
            // foreach (var dungeon in dungeons)
            // {
            //     var dungeonChunk = CreateNewChunk(dungeon.name, (int)dungeon.transform.position.x,
            //         (int)dungeon.transform.position.z);
            //     dungeonChunk.AddObject(dungeon);
            //     chunks.Add(dungeonChunk);
            // }

            foreach (var chunk in dirtyChunks)
            {
                EditorSceneManager.SaveScene(chunk.scene, $"Assets/Scenes/Chunks/{chunk.Name}.unity");
            }
        }

        /// <summary>
        /// Add a new chunk to the asset
        /// </summary>
        /// <param name="x">Chunk position in chunk coordinates</param>
        /// <param name="z">Chunk position in chunk coordinates</param>
        /// <returns></returns>
        public Chunk AddChunk(int x, int z)
        {
            var chunk = CreateNewChunk("Chunk", x, z);
            //AssetDatabase.ImportAsset($"Assets/Scenes/Chunks/{chunk.Name}.unity", ImportAssetOptions.ForceSynchronousImport);
            chunks.Add(chunk);
            SortChunks();
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            return chunk;
        }

        // [Button("Test")]
        // public void Test()
        // {
        //     CreateNewChunk("a", 0, 0);
        // }
        
        private Chunk CreateNewChunk(string name, int x, int z)
        {
            var chunkName = $"{name} {x}, {z}";
            var chunkScene = SceneManager.GetSceneByPath($"Assets/Scenes/Chunks/{chunkName}.unity");
            if (!chunkScene.IsValid())
            {
                chunkScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                chunkScene.name = chunkName;
                EditorSceneManager.SaveScene(chunkScene, $"Assets/Scenes/Chunks/{chunkName}.unity");
            }

            var chunk = new Chunk();
            chunk.Name = chunkName;
            int minX = -(chunksX / 2) * chunkWidth;
            int minZ = -(chunksZ / 2) * chunkDepth;
            chunk.min = new Vector3(minX + x * chunkWidth, 0, minZ + z * chunkDepth);
            chunk.max = new Vector3(chunk.min.x + chunkWidth, 0, chunk.min.z + chunkDepth);
            chunk.x = x;
            chunk.z = z;
            
            return chunk;
        }

        //[Button("Clear empty chunks")]
        public void DeleteEmptyChunks()
        {
            var toDelete = new List<Chunk>();
            foreach (var chunk in chunks)
            {
                if (chunk.scene.rootCount == 0)
                {
                    toDelete.Add(chunk);
                }
            }

            foreach (var chunk in toDelete)
            {
                chunks.Remove(chunk);
            }
            
            
            var currentScenes = EditorBuildSettings.scenes;
            
            foreach(var scene in currentScenes)
            {
                var filename = Path.GetFileNameWithoutExtension(scene.path);
                if (!chunks.Exists(c => c.Name == filename))
                {
                    File.Delete(scene.path);
                }
            }

            var filteredScenes = currentScenes.Where(ebss => File.Exists(ebss.path)).ToArray();
            EditorBuildSettings.scenes = filteredScenes;
            
            SortChunks();
        }

        private void OnDrawGizmosSelected()
        {
            // Draws a blue line from this transform to the target
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(transform.position, new Vector3(500, 0, 500));
            int minX = -(chunksX / 2) * chunkWidth;
            int minZ = -(chunksZ / 2) * chunkDepth;
            foreach(var chunk in chunks)
            {
                var chunkMin = new Vector3(minX + chunk.x * chunkWidth, 0, minZ + chunk.z * chunkDepth);
                var chunkMax = new Vector3(chunkMin.x + chunkWidth, 0, chunkMin.z + chunkDepth);

                var topLeft = new Vector3(chunkMin.x, 10, chunkMin.z);
                var bottomLeft = new Vector3(chunkMin.x, 10, chunkMax.z);
                var topRight = new Vector3(chunkMax.x, 10, chunkMin.z);
                var bottomRight = new Vector3(chunkMax.x, 10, chunkMax.z);
                
                Gizmos.DrawLine(topLeft, bottomLeft);
                Gizmos.DrawLine(bottomLeft, bottomRight);
                Gizmos.DrawLine(topRight, bottomRight);
                Gizmos.DrawLine(topRight, topLeft);
            }
        }
        #endif
    }
}