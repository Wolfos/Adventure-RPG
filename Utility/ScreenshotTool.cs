﻿using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{
	private void Update()
	{
		// TODO: Update for new input system
		// if (Input.GetKeyDown(KeyCode.S))
		// {
		// 	ScreenCapture.CaptureScreenshot("Screenshot.png");
		// }
	}

	[Button("Take Screenshot")]
	public void TakeScreenshot()
	{
		ScreenCapture.CaptureScreenshot("Screenshot.png");
	}
}
