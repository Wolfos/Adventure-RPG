using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotTool : MonoBehaviour
{
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.S))
		{
			ScreenCapture.CaptureScreenshot("Screenshot.png");
		}
	}
}
