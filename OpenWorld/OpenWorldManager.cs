using System.Collections.Generic;
#if UNITY_EDITOR
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEditor.SceneManagement;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenWorld
{
    [CreateAssetMenu(menuName = "Open World Manager")]
    public class OpenWorldManager : ScriptableObject
    {
        public int chunkWidth, chunkDepth;
        public int chunksX, chunksZ;
        public List<Chunk> chunks;

        public Object worldScene;

        public Chunk GetChunkByPosition(float x, float z)
        {
            int chunkX = Mathf.FloorToInt(x / chunkWidth) +(chunksX / 2);
            int chunkZ = Mathf.FloorToInt(z / chunkDepth) +(chunksZ / 2);

            return GetChunkByChunkPosition(chunkX, chunkZ);
        }
        
        private Chunk GetChunkByChunkPosition(int x, int z)
        {
            return chunks[z + x * chunksX];
        }

        public List<Chunk> GetChunkAndAdjacent(float x, float z)
        {
            var ret = new List<Chunk>();
            int chunkX = Mathf.FloorToInt(x / chunkWidth) +(chunksX / 2);
            int chunkZ = Mathf.FloorToInt(z / chunkDepth) +(chunksZ / 2);

            ret.Add(GetChunkByChunkPosition(chunkX, chunkZ));
            ret.Add(GetChunkByChunkPosition(chunkX-1, chunkZ-1));
            ret.Add(GetChunkByChunkPosition(chunkX, chunkZ-1));
            ret.Add(GetChunkByChunkPosition(chunkX-1, chunkZ));
            ret.Add(GetChunkByChunkPosition(chunkX+1, chunkZ));
            ret.Add(GetChunkByChunkPosition(chunkX+1, chunkZ+1));
            ret.Add(GetChunkByChunkPosition(chunkX, chunkZ+1));
            ret.Add(GetChunkByChunkPosition(chunkX-1, chunkZ+1));
            ret.Add(GetChunkByChunkPosition(chunkX+1, chunkZ-1));
            
            return ret;
        }
        
#if UNITY_EDITOR
        [Button("Generate Chunks")]
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
                    var chunk = GetChunkByPosition(child.position.x, child.position.z);
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

        private Chunk CreateNewChunk(string name, int x, int z)
        {
            var chunkName = $"{name} {x}, {z}";
            var chunkScene = SceneManager.GetSceneByPath($"Assets/Scenes/Chunks/{chunkName}.unity");
            bool isNew = false;
            if (!chunkScene.IsValid())
            {
                chunkScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                chunkScene.name = chunkName;
                isNew = true;
            }

            var chunk = new Chunk();
            chunk.Name = chunkName;
            int minX = -(chunksX / 2) * chunkWidth;
            int minZ = -(chunksZ / 2) * chunkDepth;
            chunk.min = new Vector3(minX + x * chunkWidth, 0, minZ + z * chunkDepth);
            chunk.max = new Vector3(chunk.min.x + chunkWidth, 0, chunk.min.z + chunkDepth);

            if (isNew)
            {
                EditorSceneManager.SaveScene(chunk.scene, $"Assets/Scenes/Chunks/{chunk.Name}.unity");
            }

            return chunk;
        }

        private void OnDrawGizmosSelected()
        {
            // Draws a blue line from this transform to the target
            //Gizmos.color = Color.blue;
            //Gizmos.DrawLine(transform.position, new Vector3(500, 0, 500));
            int minX = -(chunksX / 2) * chunkWidth;
            int minZ = -(chunksZ / 2) * chunkDepth;
            for (int x = 0; x < chunksX; x++)
            {
                for (int z = 0; z < chunksZ; z++)
                {
                    var chunkMin = new Vector3(minX + x * chunkWidth, 0, minZ + z * chunkDepth);
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
        }
        #endif
    }
}