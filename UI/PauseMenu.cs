using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public class PauseMenu : MonoBehaviour
    {
        private static PauseMenu instance;
        
        void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }
        
        public static void ToggleActive()
        {
            instance.gameObject.SetActive(!instance.gameObject.activeSelf);

            Time.timeScale = instance.gameObject.activeSelf ? 0 : 1;
        }
    }
}