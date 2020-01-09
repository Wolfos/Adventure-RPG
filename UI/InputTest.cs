using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputTest : MonoBehaviour
{
    void Update()
    {
        GetComponent<Text>().text = "Submit button pressed: " + Input.GetButton("Submit");
    }
}
