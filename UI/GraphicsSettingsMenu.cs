using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.UI;
using Utility;

public static class DropdownExtension
{
    public static void AddOption(this Dropdown dropdown, string text)
    {
        dropdown.options.Add(new()
        {
            text = text
        });
    }
}
public class GraphicsSettingsMenu : MonoBehaviour
{
    [SerializeField] private Dropdown resolution;
    [SerializeField] private Toggle vsync;
    [SerializeField] private Dropdown upscaling;
    [SerializeField] private Dropdown lighting;
    [SerializeField] private Dropdown reflections;
    [SerializeField] private Dropdown shadowQuality;
    [SerializeField] private Dropdown objectQuality;
    [SerializeField] private Dropdown motionBlurQuality;
    [SerializeField] private Slider motionBlurAmount;
    [SerializeField] private Dropdown fogQuality;
    [SerializeField] private Text motionBlurAmountText;
    [SerializeField] private Text fpsCounter;
    [SerializeField] private Text averageFPSCounter;

    private float[] averageFPSArray = new float[60];
    private int averageFPSIterator;

    private void Awake()
    {
        LoadOptions();
        
        int i = 0;
        foreach (var res in Screen.resolutions)
        {
            resolution.AddOption($"{res.width.ToString()} x {res.height.ToString()}");
            if (res.width == Screen.currentResolution.width && res.height == Screen.currentResolution.height)
            {
                resolution.SetValueWithoutNotify(i);
            }
            i++;
        }

        // TODO: Localize
        // It's very important that the order of these settings match their respective enums (defined in GraphicsSettings.cs)
        upscaling.AddOption("None");
        upscaling.AddOption("FidelityFX Super Resolution");
        if (HDDynamicResolutionPlatformCapabilities.DLSSDetected)
        {
            upscaling.AddOption("DLSS Quality");
            upscaling.AddOption("DLSS Balanced");
            upscaling.AddOption("DLSS Performance");
            upscaling.AddOption("DLSS Ultra Performance");
        }
        
        lighting.AddOption("Direct only");
        lighting.AddOption("Screen Space Global Illumination");
        if (SystemInfo.supportsRayTracing)
        {
            lighting.AddOption("Ray traced GI (low)");
            lighting.AddOption("Ray traced GI (high)");
            //lighting.AddOption("Ray traced GI (high)");
        }
        
        reflections.AddOption("None");
        reflections.AddOption("Screen space");
        if (SystemInfo.supportsRayTracing)
        { 
            reflections.AddOption("Ray tracing low");
            reflections.AddOption("Ray tracing high");
        }
        
        shadowQuality.AddOption("Low");
        shadowQuality.AddOption("Medium");
        shadowQuality.AddOption("High");
        shadowQuality.AddOption("Ultra");
        
        objectQuality.AddOption("Low");
        objectQuality.AddOption("Medium");
        objectQuality.AddOption("High");
        
        motionBlurQuality.AddOption("Low");
        motionBlurQuality.AddOption("Medium");
        motionBlurQuality.AddOption("High");
        
        fogQuality.AddOption("Low");
        fogQuality.AddOption("Medium");
        fogQuality.AddOption("High");
        
        resolution.onValueChanged.AddListener(OnResolutionChanged);
        upscaling.onValueChanged.AddListener(OnUpscalingChanged);
        lighting.onValueChanged.AddListener(OnLightingQualityChanged);
        reflections.onValueChanged.AddListener(OnReflectiongQualityChanged);
        shadowQuality.onValueChanged.AddListener(OnShadowQualityChanged);
        objectQuality.onValueChanged.AddListener(OnObjectQualityChanged);
        vsync.onValueChanged.AddListener(OnVsyncChanged);
        motionBlurQuality.onValueChanged.AddListener(OnMotionBlurQualityChanged);
        motionBlurAmount.onValueChanged.AddListener(OnMotionBlurAmountChanged);
        fogQuality.onValueChanged.AddListener(OnFogQualityChanged);
        
        LoadOptions();
    }

    private void LoadOptions()
    {
        // Resolution is handled in Awake loop
        
        vsync.SetIsOnWithoutNotify(QualitySettings.vSyncCount == 1);
        upscaling.SetValueWithoutNotify(PlayerPrefs.GetInt("UpscalingMode", 0));
        reflections.SetValueWithoutNotify(PlayerPrefs.GetInt("ReflectionQuality", 0));
        lighting.SetValueWithoutNotify(PlayerPrefs.GetInt("LightingQuality", 0));
        shadowQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("ShadowQuality", 0));
        objectQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("LodQuality", 1));
        motionBlurQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("MotionBlurQuality", 2));
        var mbAmount = PlayerPrefs.GetFloat("MotionBlurAmount", 1);
        motionBlurAmount.SetValueWithoutNotify(mbAmount);
        motionBlurAmountText.text = mbAmount.ToString("0.00");
        fogQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("FogQuality", 2));
    }

    private void Update()
    {
        float fps = 1.0f / Time.unscaledDeltaTime;
        if (averageFPSIterator is 0 or 30)
        {
            fpsCounter.text = $"FPS: {Mathf.CeilToInt(fps)}";
        }

        averageFPSArray[averageFPSIterator] = fps;
        averageFPSIterator++;
        if (averageFPSIterator >= averageFPSArray.Length) averageFPSIterator = 0;

        float totalFPS = 0;
        for (int i = 0; i < averageFPSArray.Length; i++)
        {
            totalFPS += averageFPSArray[i];
        }

        float averageFPS = totalFPS / averageFPSArray.Length;
        averageFPSCounter.text = $"Average: {Mathf.CeilToInt(averageFPS)}";
    }


    private void OnDestroy()
    {
        resolution.onValueChanged.RemoveListener(OnResolutionChanged);
    }

    private void OnResolutionChanged(int option)
    {
        var res = Screen.resolutions[option];
        GraphicsSettings.SetResolution(res.width, res.height, true);
    }

    private void OnUpscalingChanged(int option)
    {
        GraphicsSettings.SetUpscaling((UpscalingMode)option);
    }

    private void OnLightingQualityChanged(int option)
    {
        GraphicsSettings.SetLightingQuality((LightingQualityMode)option);
    }
    
    private void OnReflectiongQualityChanged(int option)
    {
        GraphicsSettings.SetReflectionQuality((ReflectionQualityMode)option);
    }
    
    private void OnShadowQualityChanged(int option)
    {
        GraphicsSettings.SetShadowQuality((ShadowQualityMode)option);
    }
    
    
    private void OnObjectQualityChanged(int option)
    {
        GraphicsSettings.SetLodQuality(option);
    }

    private void OnVsyncChanged(bool vsync)
    {
        GraphicsSettings.SetVsync(vsync);
    }
    
    private void OnMotionBlurQualityChanged(int option)
    {
        GraphicsSettings.SetMotionBlurQuality(option);
    }
    
    private void OnMotionBlurAmountChanged(float amount)
    {
        GraphicsSettings.SetMotionBlurIntensity(amount);
        motionBlurAmountText.text = amount.ToString("0.00");
    }
    
    private void OnFogQualityChanged(int option)
    {
        GraphicsSettings.SetFogQuality(option);
    }
}
