using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;
using Utility;

namespace UI
{
    [RequireComponent(typeof(UIDocument))]
    public class CommandConsole : MonoBehaviour
    {
        private void Start()
        {
            gameObject.SetActive(false);
            EventManager.OnToggleCommandConsole += OnToggleCommandConsole;
        }

        private void OnDestroy()
        {
            EventManager.OnToggleCommandConsole -= OnToggleCommandConsole;
        }

        private void OnToggleCommandConsole(InputAction.CallbackContext context)
        {
            if (context.started && Debug.isDebugBuild)
            {
                gameObject.SetActive(!gameObject.activeSelf);
                Player.PlayerControls.SetInputActive(!gameObject.activeSelf);
            }

            
        }
    }
}