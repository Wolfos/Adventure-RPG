using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerMenu : MonoBehaviour
    {
        private static PlayerMenu instance;
        [SerializeField] private Button[] topPanelButtons;
        private int currentButton;
        public static bool isActive;

        public static void ToggleActive()
        {
            instance.gameObject.SetActive(!instance.gameObject.activeSelf);
            isActive = instance.gameObject.activeSelf;
            Time.timeScale = instance.gameObject.activeSelf ? 0 : 1;
        }

        private void Awake()
        {
            instance = this;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        { 
            SetActiveButtonColor();
        }

        private void SetActiveButtonColor()
        {
            if (!InputMapper.usingController) return;
            
            int i = 0;
            foreach (Button button in topPanelButtons)
            {
                if (i == currentButton)
                {
                    var colors = button.colors;
                    colors.normalColor = button.colors.highlightedColor;
                    button.colors = colors;
                }
                else
                {
                    var colors = button.colors;
                    colors.normalColor = button.colors.disabledColor;
                    button.colors = colors;
                }
                i++;
            }
        }

        private void OnDestroy()
        {
            isActive = false;
        }

        private void Update()
        {
            if (InputMapper.MenuLeft())
            {
                currentButton --;
                if (currentButton < 0) currentButton = topPanelButtons.Length - 1;
                
                topPanelButtons[currentButton].onClick.Invoke();
                SetActiveButtonColor();
            }
            if (InputMapper.MenuRight())
            {
                currentButton ++;
                if (currentButton >= topPanelButtons.Length) currentButton = 0;
                
                topPanelButtons[currentButton].onClick.Invoke();
                SetActiveButtonColor();
            }
        }
    }
}