using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace World
{
    public class TimedLocalFog : MonoBehaviour
    {
        [SerializeField] private LocalVolumetricFog fog;
        [SerializeField] private AnimationCurve fogCurve;
        [SerializeField] private float minDistance;
        [SerializeField] private float maxDistance;

        public void Update()
        {
            var time = TimeManager.RealTime() / 24;
            fog.parameters.meanFreePath = Mathf.Lerp(minDistance, maxDistance, fogCurve.Evaluate(time));
        }
    }
}