using System;
using Character;
using UnityEngine.InputSystem;

namespace Utility
{
	public static class EventManager
	{
		// New health, max health
		public static Action<float, float> OnPlayerHealthChanged;
		public static Action<float, float> OnPlayerStaminaChanged;
		public static Action<float, float> OnPlayerManaChanged;

		// The NPC we're talking to
		public static Action<CharacterBase> OnDialogueStarted;
		public static Action OnDialogueEnded;
		
		// 
		// Input
		//
		public static Action<InputAction.CallbackContext> OnMove;
		public static Action<InputAction.CallbackContext> OnJump;
		public static Action<InputAction.CallbackContext> OnDodge;
		public static Action<InputAction.CallbackContext> OnAttack;
		public static Action<InputAction.CallbackContext> OnBlock;
		public static Action<InputAction.CallbackContext> OnInteract;
		public static Action<InputAction.CallbackContext> OnCameraMove;
		public static Action<InputAction.CallbackContext> OnCameraZoom;
		public static Action<InputAction.CallbackContext> OnPlayerMenu;
		public static Action<InputAction.CallbackContext> OnPauseMenu;
		public static Action<InputAction.CallbackContext> OnDrop;
		public static Action<InputAction.CallbackContext> OnMenuLeft;
		public static Action<InputAction.CallbackContext> OnMenuLeft2;
		public static Action<InputAction.CallbackContext> OnMenuRight;
		public static Action<InputAction.CallbackContext> OnMenuRight2;
		public static Action<InputAction.CallbackContext> OnDialogueNext;
		public static Action<InputAction.CallbackContext> OnToggleCommandConsole;
		public static Action<InputAction.CallbackContext> OnSprint;
		public static Action<InputAction.CallbackContext> OnUIMove;
		public static Action<InputAction.CallbackContext> OnUIMoveAnalog;
		public static Action<InputAction.CallbackContext> OnUIConfirm;
	}
}