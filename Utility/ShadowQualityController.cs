using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace Utility
{
	[RequireComponent(typeof(HDAdditionalLightData))]
	public class ShadowQualityController: MonoBehaviour
	{
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
			var lightData = GetComponent<HDAdditionalLightData>();
			lightData.shadowResolution.level = (int) quality;
			var contactShadowQuality = Mathf.Min((int) quality, 2);
			lightData.useContactShadow.level = contactShadowQuality;
			
		}
	}
}