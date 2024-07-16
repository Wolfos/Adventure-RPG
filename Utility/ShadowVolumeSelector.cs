using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Utility
{
    public class ShadowVolumeSelector : MonoBehaviour
    {
        [SerializeField] private Volume high;
        [SerializeField] private Volume medium;
        [SerializeField] private Volume low;
        private void Awake()
        {
            GraphicsSettings.OnShadowQualityChanged += OnShadowQualityChanged;
            OnShadowQualityChanged((ShadowQualityMode)PlayerPrefs.GetInt("ShadowQuality", 0));
        }

        private void OnDestroy()
        {
            GraphicsSettings.OnShadowQualityChanged -= OnShadowQualityChanged;
        }

        private void OnShadowQualityChanged(ShadowQualityMode quality)
        {
            switch (quality)
            {
                case ShadowQualityMode.Low:
                    low.enabled = true;
                    medium.enabled = false;
                    high.enabled = false;
                    break;
                case ShadowQualityMode.Medium:
                    low.enabled = false;
                    medium.enabled = true;
                    high.enabled = false;
                    break;
                case ShadowQualityMode.High:
                    low.enabled = false;
                    medium.enabled = false;
                    high.enabled = true;
                    break;
                case ShadowQualityMode.Ultra:
                    low.enabled = false;
                    medium.enabled = false;
                    high.enabled = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(quality), quality, null);
            }
        }
    }
}