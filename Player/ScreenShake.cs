using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player
{
    public class ScreenShake : MonoBehaviour
    {
	    private static float _shakeStartTime;
	    private static float _shakeDuration;
	    
	    private Vector3 _startPosition;
	    [SerializeField] private float shakeIntensity = 1;

	    private void Awake()
	    {
		    _startPosition = transform.localPosition;
		    _shakeStartTime = 0;
	    }

	    private void Update()
	    {
		    if (Time.time < _shakeStartTime + _shakeDuration)
		    {
			    transform.localPosition += Random.insideUnitSphere * shakeIntensity;
		    }
		    else
		    {
			    transform.localPosition = _startPosition;
		    }
	    }

	    public static void Shake(float duration)
	    {
		    _shakeStartTime = Time.time;
		    _shakeDuration = duration;
	    }
    }
}