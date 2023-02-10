using System;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class PlayerMenuWindow : Window
    {
        [SerializeField] private Button[] topPanelButtons;
        private int _currentButton;

        private void OnEnable()
        { 
            SetActiveButtonColor();
            EventManager.OnMenuLeft += OnMenuLeft;
            EventManager.OnMenuRight += OnMenuRight;
        }

        private void OnDisable()
        {
            EventManager.OnMenuLeft -= OnMenuLeft;
            EventManager.OnMenuRight -= OnMenuRight;
        }

        private void SetActiveButtonColor()
        {
            if (!InputMapper.UsingController) return;
            
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

        private void OnMenuLeft(InputAction.CallbackContext context)
        {
            _currentButton --;
            if (_currentButton < 0) _currentButton = topPanelButtons.Length - 1;
                
            topPanelButtons[_currentButton].onClick.Invoke();
            SetActiveButtonColor();
        }
        
        private void OnMenuRight(InputAction.CallbackContext context)
        {
            _currentButton ++;
            if (_currentButton >= topPanelButtons.Length) _currentButton = 0;
                
            topPanelButtons[_currentButton].onClick.Invoke();
            SetActiveButtonColor();
        }
    }
}