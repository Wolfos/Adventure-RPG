using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class SetShadowMapResolution : MonoBehaviour
{
    [SerializeField] private int resolution = 4096;
    private void Awake()
    {
        GetComponent<Light>().shadowCustomResolution = resolution;
    }
}
