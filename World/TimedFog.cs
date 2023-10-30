using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace World
{
    public class TimedFog : MonoBehaviour
    {
        [HideInInspector] public float minFog = 800; // Minimum fog for weather type
        [SerializeField] private LocalVolumetricFog fog;
        [SerializeField] private AnimationCurve fogCurve;
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;

        public void Update()
        {
            var time = TimeManager.RealTime() / 24;
            fog.parameters.meanFreePath = Mathf.Min(Mathf.Lerp(minDistance, maxDistance, fogCurve.Evaluate(time)), minFog);
        }

        public Color GetColor()
        {
            return fog.parameters.albedo;
        }

        public void SetColor(Color color)
        {
            fog.parameters.albedo = color;
        }
    }
}