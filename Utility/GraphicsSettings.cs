using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace Utility
{
	public enum UpscalingMode
	{
		None, FSR, DLSSQuality, DLSSBalanced, DLSSPerformance, DLSSUltraPerformance
	}

	public enum LightingQualityMode
	{
		NoIndirect, ScreenSpace, RayTracingLow, RaytracingMedium, RaytracingHigh
	}

	public enum ReflectionQualityMode
	{
		None, ScreenSpace, RaytracingLow, RaytracingHigh
	}

	public enum ShadowQualityMode
	{
		Low, Medium, High, Ultra
	}
	
	public class GraphicsSettings: MonoBehaviour
	{
		private static GraphicsSettings _instance;
		[SerializeField] private HDAdditionalCameraData hdCameraData;

		[SerializeField] private VolumeProfile postFXProfile;

		public static Action<ShadowQualityMode> OnShadowQualityChanged;

		private void Awake()
		{
			_instance = this;
			LoadOptions();
		}

		private void LoadOptions()
		{
			var res = Screen.currentResolution;
			SetResolution(PlayerPrefs.GetInt("ResolutionWidth", res.width), PlayerPrefs.GetInt("ResolutionHeight", res.height), true);
			
			SetUpscaling((UpscalingMode)PlayerPrefs.GetInt("UpscalingMode", 0));
			SetLightingQuality((LightingQualityMode)PlayerPrefs.GetInt("LightingQuality", 0));
			SetReflectionQuality((ReflectionQualityMode)PlayerPrefs.GetInt("ReflectionQuality", 0));
			SetShadowQuality((ShadowQualityMode)PlayerPrefs.GetInt("ShadowQuality", 0));
		}

		public static void SetResolution(int width, int height, bool fullscreen)
		{
			PlayerPrefs.SetInt("ResolutionWidth", width);
			PlayerPrefs.SetInt("ResolutionHeight", height);
			PlayerPrefs.Save();
			
			Screen.SetResolution(width, height, fullscreen);
		}

		public static void SetUpscaling(UpscalingMode mode)
		{
			PlayerPrefs.SetInt("UpscalingMode", (int)mode);
			PlayerPrefs.Save();
			switch (mode)
			{
				case UpscalingMode.None:
					_instance.hdCameraData.allowDynamicResolution = false;
					break;
				case UpscalingMode.FSR:
					_instance.hdCameraData.allowDynamicResolution = true;
					break;
				case UpscalingMode.DLSSQuality:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 0;
					break;
				case UpscalingMode.DLSSBalanced:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 1;
					break;
				case UpscalingMode.DLSSPerformance:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 2;
					break;
				case UpscalingMode.DLSSUltraPerformance:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 3;
					break;
				
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		public static void SetLightingQuality(LightingQualityMode mode)
		{
			PlayerPrefs.SetInt("LightingQuality", (int)mode);
			PlayerPrefs.Save();
			
			if (_instance.postFXProfile.TryGet<GlobalIllumination>(out var gi) == false)
			{
				Debug.LogError("No global illumination component found");
				return;
			}
			switch (mode)
			{
				case LightingQualityMode.NoIndirect:
					gi.enable.Override(false);
					break;
				case LightingQualityMode.ScreenSpace:
					gi.enable.Override(true);
					gi.tracing.Override(RayCastingMode.RayMarching);
					break;
				case LightingQualityMode.RayTracingLow:
					gi.enable.Override(true);
					gi.tracing.Override(RayCastingMode.Mixed);
					
					gi.fullResolution = false;
					gi.mode = new(RayTracingMode.Performance);
					break;
				case LightingQualityMode.RaytracingMedium:
					gi.enable.Override(true);
					gi.tracing.Override(RayCastingMode.Mixed);
					gi.fullResolution = true;
					gi.mode = new(RayTracingMode.Performance);
					break;
				case LightingQualityMode.RaytracingHigh:
					gi.enable.Override(true);
					gi.tracing.Override(RayCastingMode.RayTracing);
					gi.fullResolution = true;
					gi.mode = new(RayTracingMode.Quality);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}
		
		public static void SetReflectionQuality(ReflectionQualityMode mode)
		{
			PlayerPrefs.SetInt("ReflectionQuality", (int)mode);
			PlayerPrefs.Save();
			
			if (_instance.postFXProfile.TryGet<ScreenSpaceReflection>(out var reflection) == false)
			{
				Debug.LogError("No SSR component found");
				return;
			}
			switch (mode)
			{
				case ReflectionQualityMode.None:
					reflection.enabled.Override(false);
					reflection.enabledTransparent.Override(false);
					break;
				case ReflectionQualityMode.ScreenSpace:
					reflection.enabled.Override(true);
					reflection.enabledTransparent.Override(true);
					reflection.tracing.Override(RayCastingMode.RayMarching);
					break;
				case ReflectionQualityMode.RaytracingLow:
					reflection.enabled.Override(true);
					reflection.enabledTransparent.Override(true);
					reflection.tracing.Override(RayCastingMode.Mixed);
					reflection.fullResolution = false;
					reflection.mode = new(RayTracingMode.Performance);
					break;
				case ReflectionQualityMode.RaytracingHigh:
					reflection.enabled.Override(true);
					reflection.enabledTransparent.Override(true);
					reflection.tracing.Override(RayCastingMode.Mixed);
					reflection.fullResolution = true;
					reflection.mode = new(RayTracingMode.Performance);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}
		
		public static void SetShadowQuality(ShadowQualityMode mode)
		{
			PlayerPrefs.SetInt("ShadowQuality", (int)mode);
			PlayerPrefs.Save();
			
			OnShadowQualityChanged?.Invoke(mode);
		}
	}
}