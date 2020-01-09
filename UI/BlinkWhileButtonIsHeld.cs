using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;

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

	private void Update()
	{
		bool buttonHeld = false;
		switch (button)
		{
			case Button.InteractionButton:
				buttonHeld = InputMapper.InteractionButtonHeld();
				break;
			case Button.DropButton:
				buttonHeld = InputMapper.DropButtonHeld();
				break;
			case Button.InventoryButton:
				buttonHeld = InputMapper.InventoryButtonHeld();
				break;
		}
		
		if (buttonHeld)
		{
			graphic.color = blinkColor;
		}
		else
		{
			graphic.color = Color.white;
		}
	}
}
