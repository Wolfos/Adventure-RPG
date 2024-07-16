using System;
using System.Collections;
using UnityEngine;

namespace UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private float fadeSpeed = 2;
        [SerializeField] private CanvasGroup canvasGroup;

        private static LoadingScreen _instance;
        public static bool IsDone { get; private set; }
        
        private void Awake()
        {
            _instance = this;
            gameObject.SetActive(false);
        }

        public static void StartLoading()
        {
            IsDone = false;

            _instance.gameObject.SetActive(true);
            _instance.StartCoroutine(_instance.FadeIn());
        }
        
        public static void EndLoading()
        {
            IsDone = false;

            _instance.StartCoroutine(_instance.FadeOut());
        }

        private IEnumerator FadeIn()
        {
            for (float t = 0; t < 1; t += fadeSpeed * Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = t;
                yield return null;
            }

            canvasGroup.alpha = 1;
            IsDone = true;
        }
        
        private IEnumerator FadeOut()
        {
            for (float t = 0; t < 1; t += fadeSpeed * Time.unscaledDeltaTime)
            {
                canvasGroup.alpha = 1 - t;
                yield return null;
            }
            
            gameObject.SetActive(false);
            IsDone = true;
        }
    }
}