using System;
using System.Collections;
using System.Collections.Generic;
using UI;
using UnityEngine;

public class RenderMap : MonoBehaviour
{
    private void Start()
    {
        UIBase.Disable();
        ScreenCapture.CaptureScreenshot("Map.png");
    }
}
