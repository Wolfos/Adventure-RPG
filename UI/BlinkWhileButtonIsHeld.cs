using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

public class BlinkWhileButtonIsHeld : MonoBehaviour
{
	private Graphic graphic;
	[SerializeField] private Color blinkColor;

	private enum Button
	{
		InteractionButton, DropButton, InventoryButton
	}

	[SerializeField] private Button button;

	private void Start()
	{
		graphic = GetComponent<Graphic>();
	}

	private void OnEnable()
	{
		switch (button)
		{
			case Button.InteractionButton:
				EventManager.OnInteract += InputCallback;
				break;
			case Button.DropButton:
				EventManager.OnDrop += InputCallback;
				break;
			case Button.InventoryButton:
				EventManager.OnPlayerMenu += InputCallback;
				break;
		}
	}

	private void OnDisable()
	{
		switch (button)
		{
			case Button.InteractionButton:
				EventManager.OnInteract -= InputCallback;
				break;
			case Button.DropButton:
				EventManager.OnDrop -= InputCallback;
				break;
			case Button.InventoryButton:
				EventManager.OnPlayerMenu -= InputCallback;
				break;
		}
	}

	private void InputCallback(InputAction.CallbackContext context)
	{
		switch (context.phase)
		{
			case InputActionPhase.Started:
				graphic.color = blinkColor;
				break;
			case InputActionPhase.Canceled:
				graphic.color = Color.white;
				break;
		}
	}
}
