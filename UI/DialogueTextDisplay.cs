using System;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace UI
{
	public class DialogueTextDisplay : MonoBehaviour
	{
		[SerializeField] private Text textField;

		private Action<int> _callback;
		private float _continueTimer = 0;

		private bool _enabled;

		private void OnEnable()
		{
			EventManager.OnDialogueNext += OnDialogueNext;
			_enabled = true;
		}
		
		private void OnDisable()
		{
			// Workaround. OnDisable is sometimes not called here so I call it manually.
			// This method will usually be called twice as a result
			if (!_enabled) return;
			
			EventManager.OnDialogueNext -= OnDialogueNext;
			_enabled = false;
		}

		private void OnDialogueNext(InputAction.CallbackContext context)
		{
			if (context.phase == InputActionPhase.Canceled)
			{
				if (_continueTimer > 0.2f)
				{
					_continueTimer = 0;
					_callback?.Invoke(0);
				}
			}
		}

		private void Update()
		{
			_continueTimer += Time.unscaledDeltaTime;
		}

		public void Activate(string text, Action<int> callback)
		{
			gameObject.SetActive(true);
			textField.text = text;
			this._callback = callback;
		}

		public void DeActivate()
		{
			if (gameObject != null)
			{
				gameObject.SetActive(false);
				OnDisable(); // Workaround for bug in Unity
			}
		}
	}
}