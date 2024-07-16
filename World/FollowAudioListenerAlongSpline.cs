using System;
using System.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace World
{
    public class FollowAudioListenerAlongSpline : MonoBehaviour
    {
	    [SerializeField] private SplineContainer[] splines;
	    private Transform _followTransform;
	    
	    private void Awake()
	    {
		    _followTransform = FindAnyObjectByType<AudioListener>().transform;

		    StartCoroutine(Loop());
	    }

	    private IEnumerator Loop()
	    {
		    while (true)
		    {
			    if (_followTransform == null) yield break;

			    var transform1 = transform;
			    float3 nearestPoint = transform1.position;
			    var nearestDistance = Mathf.Infinity;
			    var selectedSpline = transform1;
			    foreach (var spline in splines)
			    {
				    foreach (var s in spline.Splines)
				    {
					    var distance = SplineUtility.GetNearestPoint(s,
						    _followTransform.position - spline.transform.position,
						    out var point, out _, resolution: 2);
					    if (distance < nearestDistance)
					    {
						    nearestDistance = distance;
						    nearestPoint = point;
						    selectedSpline = spline.transform;
					    }

					    yield return null;
				    }
			    }

			    transform.position = (Vector3) nearestPoint + selectedSpline.position;
		    }
	    }
    }
}