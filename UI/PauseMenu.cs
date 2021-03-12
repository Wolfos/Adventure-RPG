using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        private static PauseMenu instance;
        public static bool isActive;
        
        void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }
        
        public static void ToggleActive()
        {
            instance.gameObject.SetActive(!instance.gameObject.activeSelf);
            isActive = instance.gameObject.activeSelf;
            Time.timeScale = instance.gameObject.activeSelf ? 0 : 1;
        }
        
        private void OnDestroy()
        {
            isActive = false;
        }
    }
}