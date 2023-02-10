using System;
using UnityEngine;
using UnityEngine.VFX;

namespace FX
{
    public class WaterRipples: MonoBehaviour
    {
        [SerializeField] private GameObject ripplePrefab;
        [SerializeField] private float rippleDistance = 0.5f;
        private Vector3 _previousPosition;

        private void OnTriggerStay(Collider other)
        {
            if (other.CompareTag("LocalPlayer"))
            {
                var playerPosition = other.transform.position;
                if (Vector3.SqrMagnitude(playerPosition - _previousPosition) > rippleDistance * rippleDistance)
                {
                    Instantiate(ripplePrefab, other.transform.position, ripplePrefab.transform.rotation);
                    _previousPosition = playerPosition;
                }
            }
        }
    }
}