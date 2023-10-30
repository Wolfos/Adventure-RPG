using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class Rain : MonoBehaviour
    {
        private static ParticleSystem _particleSystem;
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
        }

        public static void StartRain(bool instant = false)
        {
            var particleSystemMain = _particleSystem.main;
            particleSystemMain.prewarm = instant;
            _particleSystem.Play();
        }

        public static void StopRain(bool instant = false)
        {
            _particleSystem.Stop(instant, instant ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
        }
    }
}