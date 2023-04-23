using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using Utility;

namespace Player
{
	public class InputMapper: MonoBehaviour
	{
		private PlayerInput _playerInput;

		public static bool UsingController { get; private set; }

		private void Awake()
		{
			_playerInput = GetComponent<PlayerInput>();
			UsingController = _playerInput.currentControlScheme.ToLower().Contains("gamepad");
			InputUser.onChange += (_, change, _) =>
			{
				if (change is InputUserChange.ControlSchemeChanged)
				{
					UsingController = _playerInput.currentControlScheme.ToLower().Contains("gamepad");
				}
			};
		}

		// TODO: Cache for performance
		public static Vector2 MousePosition => Mouse.current.position.ReadValue();
		
		public void OnMove(InputAction.CallbackContext context)
		{
			EventManager.OnMove?.Invoke(context);
		}

		public void OnJump(InputAction.CallbackContext context)
		{
			EventManager.OnJump?.Invoke(context);
		}
		
		public void OnCameraMove(InputAction.CallbackContext context)
		{
			EventManager.OnCameraMove?.Invoke(context);
		}
		
		public void OnCameraZoom(InputAction.CallbackContext context)
		{
			EventManager.OnCameraZoom?.Invoke(context);
		}

		public void OnDodge(InputAction.CallbackContext context)
		{
			EventManager.OnDodge?.Invoke(context);
		}
		
		public void OnInteract(InputAction.CallbackContext context)
		{
			EventManager.OnInteract?.Invoke(context);
		}
		
		public void OnAttack(InputAction.CallbackContext context)
		{
			EventManager.OnAttack?.Invoke(context);
		}
		
		public void OnBlock(InputAction.CallbackContext context)
		{
			EventManager.OnBlock?.Invoke(context);
		}

		public void OnPlayerMenu(InputAction.CallbackContext context)
		{
			EventManager.OnPlayerMenu?.Invoke(context);
		}

		public void OnPauseMenu(InputAction.CallbackContext context)
		{
			EventManager.OnPauseMenu?.Invoke(context);
		}

		public void OnDrop(InputAction.CallbackContext context)
		{
			EventManager.OnDrop?.Invoke(context);
		}
		
		public void OnMenuLeft(InputAction.CallbackContext context)
		{
			EventManager.OnMenuLeft?.Invoke(context);
		}
		
		public void OnMenuRight(InputAction.CallbackContext context)
		{
			EventManager.OnMenuRight?.Invoke(context);
		}
		
		public void OnDialogueNext(InputAction.CallbackContext context)
		{
			EventManager.OnDialogueNext?.Invoke(context);
		}

		public void OnToggleCommandConsole(InputAction.CallbackContext context)
		{
			EventManager.OnToggleCommandConsole?.Invoke(context);
		}
	}
}