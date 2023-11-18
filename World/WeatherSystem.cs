using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using WolfRPG.Core.CommandConsole;
using Random = Unity.Mathematics.Random;

namespace World
{
    
    public class SetWeatherCommand : IConsoleCommand
    {
        public string Word => "setweather";
        public ConsoleArgumentType[] Arguments { get; } = { ConsoleArgumentType.String };
        public void Execute(object[] arguments, Action<string> onError)
        {
            var argument = arguments[0] as string;
            if (argument == "rain")
            {
                WeatherSystem.SetWeatherInstantStatic(WeatherSystem.WeatherType.Rainy);
            }
            if (argument == "overcast")
            {
                WeatherSystem.SetWeatherInstantStatic(WeatherSystem.WeatherType.Overcast);
            }
            if (argument is "sunny" or "clear")
            {
                WeatherSystem.SetWeatherInstantStatic(WeatherSystem.WeatherType.Sunny);
            }
            if (argument == "cloudy")
            {
                WeatherSystem.SetWeatherInstantStatic(WeatherSystem.WeatherType.Cloudy);
            }
        }
    }
    public class WeatherSystem : SaveableObject
    {
        private class WeatherSystemData: ISaveData
        {
            public int LastChangeDay { get; set; }
            public int LastChangeHour { get; set; }
            public int NextChange { get; set; } // Hours since last change
            public WeatherType WeatherType { get; set; }
        }

        public enum WeatherType
        {
            Sunny, Cloudy, Overcast, Rainy
        }

        [Serializable]
        private class WeatherTypeDefinition
        {
            public WeatherType type;
            public float chance = 50;
            public float minFog = 800;
            public float cloudShapeFactor = 0.9f;
            public bool backgroundClouds;
            public float cloudShadows;
            public Color fogColor;
        }

        [SerializeField] private WeatherTypeDefinition[] weatherTypes;
        [SerializeField] private int minChangeTime = 1; // In hours
        [SerializeField] private int maxChangeTime = 24; // In hours
        [SerializeField] private TimedFog fogManager;
        [SerializeField] private VolumeProfile environmentProfile;
        [SerializeField] private float lerpTime;
        [SerializeField] private float rainStartTime = 60;
        [SerializeField] private WeatherType defaultWeather;
        private Random random;
        private static WeatherSystem _instance;



        private WeatherSystemData _data = new();
        
        private void Start()
        {
            CommandConsole.RegisterCommand(new SetWeatherCommand());
            _instance = this;
            var seed = (uint)DateTime.Now.Ticks;
            random = new (seed);
            if (SaveGameManager.HasData(id))
            {
                _data = SaveGameManager.GetData(id) as WeatherSystemData;
                SetWeatherInstant(_data.WeatherType);
            }
            else
            {
                _data = new();
                SaveGameManager.Register(id, _data);
                
                SetWeatherInstant(defaultWeather);
                SetRandomTime();
            }
        }

        private void SetRandomTime()
        {
            TimeManager.GetTime(out var day, out var hour, out _);
            _data.LastChangeDay = day;
            _data.LastChangeHour = hour;

            _data.NextChange = random.NextInt(minChangeTime, maxChangeTime);
            Debug.Log($"Next weather in {_data.NextChange} hours");
        }
       
        private void SetRandomWeather()
        {
            SetRandomTime();
            
            // Choose random weather type
            WeatherTypeDefinition weatherType = null;
            {
                float totalChance = 0;
                foreach (var weather in weatherTypes)
                {
                    totalChance += weather.chance;
                }

                var randomChance = random.NextFloat(0, totalChance);

                float chance = 0;
                foreach (var weather in weatherTypes)
                {
                    chance += weather.chance;
                    if (chance >= randomChance)
                    {
                        weatherType = weather;
                        break;
                    }
                }
            }

            _data.WeatherType = weatherType.type;
            
            Debug.Log($"Chose weather {weatherType.type}");
            SetWeather(weatherType);
        }

        private void Update()
        {
            var hoursPassed =
                TimeManager.GetHoursPassedSince(_data.LastChangeDay, _data.LastChangeHour);
            
            if (hoursPassed >= _data.NextChange)
            {
                SetRandomWeather();
            }
        }

        public static void SetWeatherInstantStatic(WeatherType type)
        {
            _instance.SetWeatherInstant(type);
        }
        
        private void SetWeatherInstant(WeatherType type)
        {
            foreach (var weather in weatherTypes)
            {
                if (weather.type == type)
                {
                    SetWeather(weather, true);
                    return;
                }
            }
        }

        
        private void SetWeather(WeatherTypeDefinition weather, bool instant = false)
        {
            StopAllCoroutines();

            
            
            if(environmentProfile.TryGet<VolumetricClouds>(out var clouds) == false)
            {
                Debug.LogError("No volumetric clouds component found");
                return;
            }
            
            if(environmentProfile.TryGet<CloudLayer>(out var cloudLayer) == false)
            {
                Debug.LogError("No cloud layer component found");
                return;
            }

            if (instant)
            {
                fogManager.minFog = weather.minFog;
                fogManager.SetColor(weather.fogColor);
                clouds.shapeFactor.Override(weather.cloudShapeFactor);
                clouds.shadowOpacity.Override(weather.cloudShadows);
                cloudLayer.opacity.Override(weather.backgroundClouds ? 1 : 0);
                
                if (weather.type == WeatherType.Rainy)
                {
                    Rain.StartRain(true);
                }
                else
                {
                    Rain.StopRain(true);
                }
            }
            else
            {
                if (weather.type == WeatherType.Rainy)
                {
                    StartCoroutine(StartRainRoutine());
                }
                else
                {
                    StartCoroutine(StopRainRoutine());
                }
                StartCoroutine(SetFogRoutine(weather));
                StartCoroutine(SetCloudsRoutine(weather, clouds, cloudLayer));
            }
        }

        private IEnumerator SetFogRoutine(WeatherTypeDefinition weather)
        {
            var startFog = fogManager.minFog;
            var startColor = fogManager.GetColor();
            var endColor = weather.fogColor;
            for (float t = 0; t < lerpTime; t += Time.deltaTime)
            {
                fogManager.minFog = Mathf.Lerp(startFog, weather.minFog, t / lerpTime);
                fogManager.SetColor(Color.Lerp(startColor, endColor, t / lerpTime));
                yield return null;
            }
        }
        
        private IEnumerator SetCloudsRoutine(WeatherTypeDefinition weather, VolumetricClouds clouds, CloudLayer cloudLayer)
        {
            var startShapeFactor = clouds.shapeFactor.value;
            var startCloudLayerOpacity = cloudLayer.opacity.value;
            float endCloudLayerOpacity = weather.backgroundClouds ? 1 : 0;
            float startShadowOpacity = clouds.shadowOpacity.value;
            float endShadowOpacity = weather.cloudShadows;
           
            for (float t = 0; t < lerpTime; t += Time.deltaTime)
            {
                clouds.shapeFactor.Override(Mathf.Lerp(startShapeFactor, weather.cloudShapeFactor, t / lerpTime));
                clouds.shadowOpacity.Override(Mathf.Lerp(startShadowOpacity, endShadowOpacity, t / lerpTime * 2));
                cloudLayer.opacity.Override(Mathf.Lerp(startCloudLayerOpacity, endCloudLayerOpacity, t / lerpTime * 2));
                yield return null;
            }
        }

        private IEnumerator StartRainRoutine()
        {
            yield return new WaitForSeconds(rainStartTime);
            
            Rain.StartRain();
        }
        
        private IEnumerator StopRainRoutine()
        {
            yield return new WaitForSeconds(rainStartTime);
            
            Rain.StopRain();
        }
    }
}