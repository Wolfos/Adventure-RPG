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
        private static PlayerMenu _instance;
        [SerializeField] private Button[] topPanelButtons;
        private int _currentButton;
        public static bool IsActive;

        public static void ToggleActive()
        {
            var gameObject = _instance.gameObject;
            gameObject.SetActive(!gameObject.activeSelf);
            IsActive = gameObject.activeSelf;
            Time.timeScale = gameObject.activeSelf ? 0 : 1;
        }

        private void Awake()
        {
            _instance = this;
            gameObject.SetActive(false);
        }

        private void OnEnable()
        { 
            SetActiveButtonColor();
        }

        private void SetActiveButtonColor()
        {
            if (!InputMapper.usingController) return;
            
            var i = 0;
            foreach (var button in topPanelButtons)
            {
                if (i == _currentButton)
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
            IsActive = false;
        }

        private void Update()
        {
            if (InputMapper.MenuLeft())
            {
                _currentButton --;
                if (_currentButton < 0) _currentButton = topPanelButtons.Length - 1;
                
                topPanelButtons[_currentButton].onClick.Invoke();
                SetActiveButtonColor();
            }
            if (InputMapper.MenuRight())
            {
                _currentButton ++;
                if (_currentButton >= topPanelButtons.Length) _currentButton = 0;
                
                topPanelButtons[_currentButton].onClick.Invoke();
                SetActiveButtonColor();
            }
        }
    }
}