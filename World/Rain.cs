using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [RequireComponent(typeof(AudioSource))]
    public class Rain : MonoBehaviour
    {
        private static Rain _instance;
        private static ParticleSystem _particleSystem;
        private AudioSource _audioSource;
        private void Awake()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _audioSource = GetComponent<AudioSource>();
            _instance = this;
        }

        public static void StartRain(bool instant = false)
        {
            var particleSystemMain = _particleSystem.main;
            particleSystemMain.prewarm = instant;
            _particleSystem.Play();
            if (instant)
            {
                _instance._audioSource.Play();
                _instance._audioSource.volume = 1;
            }
            else
            {
                _instance.StartCoroutine(_instance.FadeInAudio());
            }

            
        }

        public static void StopRain(bool instant = false)
        {
            _particleSystem.Stop(instant, instant ? ParticleSystemStopBehavior.StopEmittingAndClear : ParticleSystemStopBehavior.StopEmitting);
            if (instant)
            {
                _instance._audioSource.Stop();
            }
            else
            {
                _instance.StartCoroutine(_instance.FadeOutAudio());
            }

        }

        private IEnumerator FadeInAudio()
        {
            _audioSource.Play();
            for (float t = 0; t < 5; t += Time.deltaTime)
            {
                _audioSource.volume = t / 5;
                yield return null;
            }

            _audioSource.volume = 1;
        }

        private IEnumerator FadeOutAudio()
        {
            for (float t = 0; t < 5; t += Time.deltaTime)
            {
                _audioSource.volume = 1 - (t / 5);
                yield return null;
            }

            _audioSource.volume = 0;
            _audioSource.Stop();
        }
    }
}