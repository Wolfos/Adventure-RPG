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
		NoIndirect, ScreenSpace, RayTracingLow, RaytracingHigh
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
		[SerializeField] private VolumeProfile fogProfile;
		[SerializeField] private HDDynamicResolution hdDynamicResolution;

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
			SetVsync(PlayerPrefs.GetInt("VSync", 1) == 1);
			SetUpscaling((UpscalingMode)PlayerPrefs.GetInt("UpscalingMode", 0));
			SetLightingQuality((LightingQualityMode)PlayerPrefs.GetInt("LightingQuality", 0));
			SetReflectionQuality((ReflectionQualityMode)PlayerPrefs.GetInt("ReflectionQuality", 0));
			SetShadowQuality((ShadowQualityMode)PlayerPrefs.GetInt("ShadowQuality", 0));
			SetLodQuality(PlayerPrefs.GetInt("LodQuality", 1));
			SetMotionBlurQuality(PlayerPrefs.GetInt("MotionBlurQuality", 2));
			SetMotionBlurIntensity(PlayerPrefs.GetFloat("MotionBlurAmount", 1));
			SetFogQuality(PlayerPrefs.GetInt("FogQuality", 1));
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
					_instance.hdDynamicResolution.enabled = false;
					break;
				case UpscalingMode.FSR:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdDynamicResolution.enabled = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = false;
					break;
				case UpscalingMode.DLSSQuality:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 2;
					_instance.hdDynamicResolution.enabled = false;
					break;
				case UpscalingMode.DLSSBalanced:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 1;
					_instance.hdDynamicResolution.enabled = false;
					break;
				case UpscalingMode.DLSSPerformance:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 0;
					_instance.hdDynamicResolution.enabled = false;
					break;
				case UpscalingMode.DLSSUltraPerformance:
					_instance.hdCameraData.allowDynamicResolution = true;
					_instance.hdCameraData.allowDeepLearningSuperSampling = true;
					_instance.hdCameraData.deepLearningSuperSamplingUseCustomQualitySettings = true;
					_instance.hdCameraData.deepLearningSuperSamplingQuality = 3;
					_instance.hdDynamicResolution.enabled = false;
					break;
				
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		public static void SetVsync(bool vsync)
		{
			var vs = vsync ? 1 : 0;
			QualitySettings.vSyncCount = vs;
			
			PlayerPrefs.SetInt("VSync", vs);
			PlayerPrefs.Save();
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
					gi.fullResolution = true; // This effect looks awful at half resolution, so no quality setting here
					break;
				case LightingQualityMode.RayTracingLow:
					gi.enable.Override(true);
					gi.tracing.Override(RayCastingMode.Mixed);
					gi.fullResolution = false;
					gi.mode.Override(RayTracingMode.Performance);
					break;
				case LightingQualityMode.RaytracingHigh:
					gi.enable.Override(true);
					//gi.tracing.Override(RayCastingMode.Mixed);
					gi.tracing.Override(RayCastingMode.RayTracing);
					
					//gi.fullResolution = true;
					gi.fullResolution = false;
					gi.mode.Override(RayTracingMode.Performance);
					break;
				// Quality mode removed because it's just too heavy
				// case LightingQualityMode.RaytracingHigh:
				// 	gi.enable.Override(true);
				// 	gi.tracing.Override(RayCastingMode.RayTracing);
				// 	gi.fullResolution = true;
				// 	gi.mode.Override(RayTracingMode.Quality);
				// 	break;
				default:
					throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
			}
		}

		public static void SetLodQuality(int quality)
		{
			PlayerPrefs.SetInt("LodQuality", quality);
			PlayerPrefs.Save();
			_instance.hdCameraData.renderingPathCustomFrameSettings.lodBiasQualityLevel = quality;
		}

		public static void SetMotionBlurQuality(int quality)
		{
			PlayerPrefs.SetInt("MotionBlurQuality", quality);
			PlayerPrefs.Save();
			if (_instance.postFXProfile.TryGet<MotionBlur>(out var motionBlur) == false)
			{
				Debug.LogError("No motion blur component found");
				return;
			}
			
			motionBlur.quality.Override(quality);
		}
		
		public static void SetMotionBlurIntensity(float intensity)
		{
			PlayerPrefs.SetFloat("MotionBlurAmount", intensity);
			PlayerPrefs.Save();
			
			if (_instance.postFXProfile.TryGet<MotionBlur>(out var motionBlur) == false)
			{
				Debug.LogError("No motion blur component found");
				return;
			}
			
			motionBlur.intensity.Override(intensity);
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
					//reflection.tracing.Override(RayCastingMode.Mixed);
					//reflection.fullResolution = true;
					// TODO: Mixed
					reflection.tracing.Override(RayCastingMode.RayTracing);
					reflection.fullResolution = false;
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

		public static void SetFogQuality(int quality)
		{
			PlayerPrefs.SetInt("FogQuality", quality);
			PlayerPrefs.Save();
			if (_instance.fogProfile.TryGet<Fog>(out var fog) == false)
			{
				Debug.LogError("No motion blur component found");
				return;
			}
			
			fog.quality.Override(quality);
		}
	}
}