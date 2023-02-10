using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private Slider slider;
        [SerializeField] private float animationSpeed = 3.0f;
        [SerializeField] private float blinkAnimationSpeed = 3.0f;
        [SerializeField] private Color blinkColor;
        [SerializeField] private Image fillImage;

        private Color _startColor;

        private void Start()
        {
            EventManager.OnPlayerHealthChanged += OnPlayerHealthChanged;
            _startColor = fillImage.color;
        }

        private void OnDestroy()
        {
            EventManager.OnPlayerHealthChanged -= OnPlayerHealthChanged;
        }

        private void OnPlayerHealthChanged(float health, float maxHealth)
        {
            fillImage.color = _startColor;
            var newValue = health / maxHealth;
            if (newValue < slider.value)
            {
                if (newValue == 0)
                {
                    StartCoroutine(BlinkAnimation(new Color(_startColor.r, _startColor.g, _startColor.b, 0)));
                }
                else
                {
                    StartCoroutine(BlinkAnimation(_startColor));
                }
            }
            StartCoroutine(SetValueAnimation(newValue));
        }

        private IEnumerator SetValueAnimation(float newValue)
        {
            var startValue = slider.value;
            for (float t = 0; t < 1; t += Time.deltaTime * animationSpeed)
            {
                slider.value = Mathf.SmoothStep(startValue, newValue, t);
                yield return null;
            }
        }

        private IEnumerator BlinkAnimation(Color targetColor)
        {
            for (float t = 0; t < 1; t += Time.deltaTime * blinkAnimationSpeed)
            {
                fillImage.color = Color.Lerp(blinkColor, targetColor, t);
                yield return null;
            }
        }
    }
}