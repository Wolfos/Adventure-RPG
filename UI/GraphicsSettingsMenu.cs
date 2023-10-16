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
    [SerializeField] private Dropdown upscaling;
    [SerializeField] private Dropdown lighting;
    [SerializeField] private Dropdown reflections;
    [SerializeField] private Dropdown shadowQuality;
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
        upscaling.AddOption("FSR (not implemented yet, will use DLSS instead)");
        if (HDDynamicResolutionPlatformCapabilities.DLSSDetected)
        {
            upscaling.AddOption("DLSS Quality");
            upscaling.AddOption("DLSS Balanced");
            upscaling.AddOption("DLSS Performance");
            upscaling.AddOption("DLSS Ultra Performance");
        }
        upscaling.SetValueWithoutNotify(PlayerPrefs.GetInt("UpscalingMode", 0));

        lighting.AddOption("Direct only");
        lighting.AddOption("Screen Space Global Illumination");
        if (SystemInfo.supportsRayTracing)
        {
            lighting.AddOption("Ray traced GI (low)");
            lighting.AddOption("Ray traced GI (medium)");
            lighting.AddOption("Ray traced GI (high)");
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

        resolution.onValueChanged.AddListener(OnResolutionChanged);
        upscaling.onValueChanged.AddListener(OnUpscalingChanged);
        lighting.onValueChanged.AddListener(OnLightingQualityChanged);
        reflections.onValueChanged.AddListener(OnReflectiongQualityChanged);
        shadowQuality.onValueChanged.AddListener(OnShadowQualityChanged);
    }

    private void LoadOptions()
    {
        // Resolution is handled in Awake loop
        
        upscaling.SetValueWithoutNotify(PlayerPrefs.GetInt("UpscalingMode", 0));
        lighting.SetValueWithoutNotify(PlayerPrefs.GetInt("LightingQuality", 0));
        shadowQuality.SetValueWithoutNotify(PlayerPrefs.GetInt("ShadowQuality", 0));
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
}
