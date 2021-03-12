using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utility {
    public class UnloadScene : MonoBehaviour
    {
        [SerializeField] private string sceneToUnload;

        private void Start()
        {
            SceneManager.UnloadSceneAsync(sceneToUnload);
        }
    }
}