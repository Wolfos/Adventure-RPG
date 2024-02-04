using System;
using System.Collections;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class HealthStaminaManaBar : MonoBehaviour
    {
        private enum BarType
        {
            Health, Stamina, Mana
        }

        [SerializeField] private BarType type;
        [SerializeField] private Slider slider;
        [SerializeField] private float animationSpeed = 3.0f;
        [SerializeField] private float blinkAnimationSpeed = 3.0f;
        [SerializeField] private Color blinkColor;
        [SerializeField] private Image fillImage;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeSpeed = 1;

        private Color _startColor;
        private bool _isDisplayed;
        private Coroutine _fadeRoutine;

        private void Start()
        {
            switch (type)
            {
                case BarType.Health:
                    EventManager.OnPlayerHealthChanged += OnValueChanged;
                    break;
                case BarType.Stamina:
                    EventManager.OnPlayerStaminaChanged += OnValueChanged;
                    break;
                case BarType.Mana:
                    EventManager.OnPlayerManaChanged += OnValueChanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            _startColor = fillImage.color;

            _isDisplayed = false;
            canvasGroup.alpha = 0;
        }

        private void OnDestroy()
        {
            switch (type)
            {
                case BarType.Health:
                    EventManager.OnPlayerHealthChanged -= OnValueChanged;
                    break;
                case BarType.Stamina:
                    EventManager.OnPlayerStaminaChanged -= OnValueChanged;
                    break;
                case BarType.Mana:
                    EventManager.OnPlayerManaChanged -= OnValueChanged;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }

        private void OnValueChanged(float value, float maxValue)
        {
            fillImage.color = _startColor;
            var newValue = (float)value / (float)maxValue;
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

            if (Math.Abs(value - maxValue) < 0.01f)
            {
                if (_isDisplayed)
                {
                    if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
                    _fadeRoutine = StartCoroutine(FadeOut());
                }
            }
            else if (_isDisplayed == false)
            {
                if(_fadeRoutine != null) StopCoroutine(_fadeRoutine);
                _fadeRoutine = StartCoroutine(FadeIn());
            }
        }

        private IEnumerator SetValueAnimation(float newValue)
        {
            var startValue = slider.value;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * animationSpeed)
            {
                slider.value = Mathf.SmoothStep(startValue, newValue, t);
                yield return null;
            }
        }

        private IEnumerator BlinkAnimation(Color targetColor)
        {
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * blinkAnimationSpeed)
            {
                fillImage.color = Color.Lerp(blinkColor, targetColor, t);
                yield return null;
            }
        }

        private IEnumerator FadeIn()
        {
            _isDisplayed = true;
            var startAlpha = canvasGroup.alpha;
            for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed * 10)
            {
                canvasGroup.alpha = Mathf.SmoothStep(startAlpha, 1, t);
                yield return null;
            }

            canvasGroup.alpha = 1;
        }
        
        private IEnumerator FadeOut()
        {
            _isDisplayed = false;
            yield return new WaitForSeconds(1.0f);
            var startAlpha = canvasGroup.alpha;
            for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
            {
                canvasGroup.alpha = Mathf.SmoothStep(startAlpha, 0, t);
                yield return null;
            }

            canvasGroup.alpha = 0;
        }
    }
}