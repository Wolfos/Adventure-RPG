using System.Collections;
using UI;
using UnityEngine;

public class RenderMap : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(1);
        UIBase.Disable();
        ScreenCapture.CaptureScreenshot("Map.png");
    }
}
