using System;
using UnityEngine;

namespace World
{
    public class FireLightEffect : MonoBehaviour
    {
        [SerializeField] private Light light;
        [SerializeField] private AnimationCurve animationCurve;
        [SerializeField] private float multiplier = 1;
        [SerializeField] private float speed = 1;
        private float _time = 0;

        private void Update()
        {
            _time += Time.deltaTime;
            if (_time > 1) _time -= 1;
            light.range = animationCurve.Evaluate(_time * speed) * multiplier;
        }
    }
}