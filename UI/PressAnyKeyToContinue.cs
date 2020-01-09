using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressAnyKeyToContinue : MonoBehaviour
{
    private void Update()
    {
        if (Input.anyKey)
        {
            Application.LoadLevel("World");
        }
    }
}
