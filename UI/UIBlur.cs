using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace UI
{
    public class UIBlur : MonoBehaviour
    {
        [SerializeField] private CustomPassVolume volume;
        private void OnEnable()
        {
            volume.targetCamera = Camera.main;
        }
    }
}