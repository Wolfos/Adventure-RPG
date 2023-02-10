using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace OpenWorld
{
    [Serializable]
    public class Chunk
    {
        public string Name;
        public Vector3 min;
        public Vector3 max;
        public int x;
        public int z;

        public Scene scene => SceneManager.GetSceneByName(Name);

        public bool FitsInChunk(Transform transform)
        {
            var xPos = transform.position.x;
            var zPos = transform.position.z;
            if (xPos > min.x && xPos <= max.x && zPos > min.z && zPos <= max.z)
            {
                return true;
            }

            return false;
        }

        public void AddObject(GameObject gameObject)
        {
            try
            {
                SceneManager.MoveGameObjectToScene(gameObject, scene);
            }
            catch
            {
                Debug.LogWarning($"Can't move {gameObject.name} to {scene.name} because that scene is not loaded");
            }
        }
    }
}