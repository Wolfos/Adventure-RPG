using System;
using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Random = Unity.Mathematics.Random;

namespace World
{
    public class WeatherSystem : SaveableObject
    {
        private class WeatherSystemData: ISaveData
        {
            public int LastChangeDay { get; set; }
            public int LastChangeHour { get; set; }
            public int NextChange { get; set; } // Hours since last change
            public WeatherType WeatherType { get; set; }
        }

        private enum WeatherType
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
        }

        [SerializeField] private WeatherTypeDefinition[] weatherTypes;
        [SerializeField] private int minChangeTime = 1; // In hours
        [SerializeField] private int maxChangeTime = 24; // In hours
        [SerializeField] private TimedFog fogManager;
        [SerializeField] private VolumeProfile environmentProfile;
        [SerializeField] private float lerpTime;
        [SerializeField] private WeatherType defaultWeather;
        private Random random;



        private WeatherSystemData _data = new();
        
        private void Start()
        {
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

            if (instant)
            {
                fogManager.minFog = weather.minFog;
                clouds.shapeFactor.Override(weather.cloudShapeFactor);
            }
            else
            {
                StartCoroutine(SetFogRoutine(weather.minFog));
                StartCoroutine(SetCloudsRoutine(weather.cloudShapeFactor, clouds));
            }
        }

        private IEnumerator SetFogRoutine(float nextFog)
        {
            var startFog = fogManager.minFog;
            for (float t = 0; t < lerpTime; t += Time.deltaTime)
            {
                fogManager.minFog = Mathf.Lerp(startFog, nextFog, t / lerpTime);
                yield return null;
            }
        }
        
        private IEnumerator SetCloudsRoutine(float nextShapeFactor, VolumetricClouds clouds)
        {
            var startShapeFactor = clouds.shapeFactor.value;
           
            for (float t = 0; t < lerpTime; t += Time.deltaTime)
            {
                clouds.shapeFactor.Override(Mathf.Lerp(startShapeFactor, nextShapeFactor, t / lerpTime));
                yield return null;
            }
        }
    }
}