using System;
using System.Collections;
using Player;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace OpenWorld
{
    public enum Location
    {
        Northlands, Westcoast, Riverlands, 
        Goldbridge
    }
    public class OverworldLocations : MonoBehaviour
    {
        [Serializable]
        public class OverworldLocation
        {
            public string name;
            public SplineContainer splineContainer;
            public Location location;
            public float3 Position { get; set; }
            public Spline Spline { get; set; }
        }
        
        [SerializeField] private OverworldLocation[] bigLocations;
        [SerializeField] private OverworldLocation[] smallLocations;
        public static OverworldLocation CurrentBigLocation;
        public static OverworldLocation CurrentSmallLocation;

        private bool _isRunning;
        private bool _bigLocationChanged;
        private bool _smallLocationChanged;
        
        private void Awake()
        {
            foreach (var location in bigLocations)
            {
                location.Position = location.splineContainer.transform.position;
                location.Spline = location.splineContainer.Spline;
            }

            foreach (var location in smallLocations)
            {
                location.Position = location.splineContainer.transform.position;
                location.Spline = location.splineContainer.Spline;
            }

            _isRunning = true;
            StartCoroutine(DetermineLocationRoutine());
        }

        private void Update()
        {
            if (_bigLocationChanged)
            {
                // TODO: Callback
                if (CurrentBigLocation == null)
                {
                    Debug.Log($"Exited big location");
                }
                else
                {
                    Debug.Log($"Entered big location {CurrentBigLocation.name}");
                }
                _bigLocationChanged = false;
            }

            if (_smallLocationChanged)
            {
                // TODO: Callback
                if (CurrentSmallLocation == null)
                {
                    Debug.Log($"Exited small location");
                }
                else
                {
                    Debug.Log($"Entered small location {CurrentSmallLocation.name}");
                }

                _smallLocationChanged = false;
            }
        }

        private IEnumerator DetermineLocationRoutine()
        {
            while (_isRunning)
            {
                float3 playerPosition = PlayerCharacter.GetPosition();

                foreach (var location in bigLocations)
                {
                    var relativePosition = playerPosition - location.Position;
                    if (IsInsideSpline(relativePosition, location.Spline))
                    {
                        if(CurrentBigLocation != location) _bigLocationChanged = true;
                        CurrentBigLocation = location;
                        
                        goto Small;
                    }

                    yield return null;
                }
                // None found
                if (CurrentBigLocation != null) _bigLocationChanged = true;
                CurrentBigLocation = null;
                
                Small:
                foreach (var location in smallLocations)
                {
                    var relativePosition = playerPosition - location.Position;
                    if (IsInsideSpline(relativePosition, location.Spline))
                    {
                        if(CurrentSmallLocation != location) _smallLocationChanged = true;
                        CurrentSmallLocation = location;
                        
                        goto Sleep;
                    }

                    yield return null;
                }
                // None found
                if (CurrentSmallLocation != null) _smallLocationChanged = true;
                CurrentSmallLocation = null;
                
                Sleep:
                yield return null;
            }
        }
        
        /// <summary>
        /// Whether a point is inside a closed spline.
        /// </summary>
        /// <param name="point">A point, local to the spline's space</param>
        /// <param name="spline">Unity spline</param>
        /// <returns>Whether the point is inside the spline</returns>
        private static bool IsInsideSpline(float3 point, Spline spline)
        {
            var bounds = spline.GetBounds();
            // Is outside of bounds?
            if (point.x < bounds.min.x || point.x > bounds.max.x ||
                point.z < bounds.min.z || point.z > bounds.max.z)
            {
                return false;
            }
            SplineUtility.GetNearestPoint(spline, point, out var splinePoint, out var t, SplineUtility.PickResolutionMin);
            spline.Evaluate(t, out _, out var tangent, out _);
			
            var cross = math.cross(math.up(), math.normalize(tangent));
            return math.dot(splinePoint - point, cross) < 0;
        }

        private void OnDestroy()
        {
            _isRunning = false;
        }
    }
}