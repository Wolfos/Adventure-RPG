using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace World
{
    public class ApplyWind : MonoBehaviour
    {
        [SerializeField] private new Rigidbody rigidbody;
        [SerializeField] private Vector3 windApplyPosition;
        [SerializeField] private float multiplier = 1;

        private Vector3 _windPosition; 
        
        private void Awake()
        {
            _windPosition = transform.TransformPoint(windApplyPosition);
        }

        private void FixedUpdate()
        {
            var position = transform.position;
            var windMain = WeatherSystem.GetWindMain(position);
            //var windPulseFrequency = WeatherSystem.GetWindPulseFrequency(position);
            //var windPulseMagnitude = WeatherSystem.GetWindPulseMagnitude(position);
            var windTurbulence = WeatherSystem.GetWindTurbulence(position);
            var windDirection = WeatherSystem.GetWindDirection(position);

            var windStrength = windMain;
            windStrength += windTurbulence * Random.value;
            windStrength *= multiplier;
            rigidbody.AddForceAtPosition(windDirection * windStrength, _windPosition);
        }

        private void OnDrawGizmosSelected()
        {
            _windPosition = transform.TransformPoint(windApplyPosition);
            Gizmos.DrawWireSphere(_windPosition, 0.1f);
        }
    }
}