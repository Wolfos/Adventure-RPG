using System;
using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI
{
    public class MapScreen : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect; // The scrollRect containing the map
        [SerializeField] private RectTransform playerLocator;
        [SerializeField] private GameObject mapMarkerPrefab;
        [SerializeField] private Vector2 topLeft;
        [SerializeField] private Vector2 bottomRight;
        [SerializeField] private Vector2 mapSize;

        private List<GameObject> _mapMarkers = new();
        private bool _initialized = false;
        private void OnEnable()
        {
            PlacePlayerLocationMarker();
            PlaceMapMarkers();
            
            StartCoroutine(ScrollToPlayerPosition());
        }

        private void PlacePlayerLocationMarker()
        {
            var playerPosition = PlayerCharacter.GetPosition(); // Vector3 in world space
            
            // Set the position of this object's RectTransform to the correct spot on the map, based on the player position
            Vector2 playerMapPosition = WorldToMapPosition(playerPosition);

            // Set the RectTransform's position
            var rectTransform = playerLocator.transform as RectTransform;
            rectTransform.anchoredPosition = playerMapPosition;
        }

        private void PlaceMapMarkers()
        {
            var allLocations = TeleportDestinations.GetAllUnlockedFastTravelLocations();
            foreach (var location in allLocations)
            {
                var marker = Instantiate(mapMarkerPrefab, mapMarkerPrefab.transform.parent);
                var anchoredPosition = WorldToMapPosition(location.Transform.position);
                marker.GetComponent<MapMarker>().Initialize(location, anchoredPosition);
                marker.SetActive(true);
                _mapMarkers.Add(marker);
            }
        }

        private void OnDisable()
        {
            foreach (var marker in _mapMarkers)
            {
                Destroy(marker);
            }
            _mapMarkers.Clear();
        }

        private Vector2 WorldToMapPosition(Vector3 worldPosition)
        {
            // Calculate the relative position within the world space map boundaries
            var relativeX = (worldPosition.x - topLeft.x) / (bottomRight.x - topLeft.x);
            var relativeY = (worldPosition.z - topLeft.y) / (bottomRight.y - topLeft.y); // Use z for Y component in world space

            // Convert relative position to map pixel position
            var mapX = relativeX * mapSize.x;
            var mapY = relativeY * mapSize.y;

            return new (mapX, -mapY);
        }

        private IEnumerator ScrollToPlayerPosition()
        {
            var mapRectTransform = scrollRect.content;
            var playerRectTransform = playerLocator.GetComponent<RectTransform>();

            // Calculate the position of the player locator in the map's coordinate system
            var playerLocalPosition = mapRectTransform.InverseTransformPoint(playerRectTransform.position);

            // Calculate the normalized position for the ScrollRect
            var mapSize = mapRectTransform.rect.size;
            var playerNormalizedPosition = new Vector2(
                playerLocalPosition.x / mapSize.x,
                playerLocalPosition.y / mapSize.y
            );

            // Set the ScrollRect's normalized position to center on the player
            var targetLocation = new Vector2 (
                Mathf.Clamp01(playerNormalizedPosition.x),
                Mathf.Clamp01(1 + playerNormalizedPosition.y)
            );

            // if (_initialized)
            // {
            //     var lastPosition = scrollRect.normalizedPosition;
            //     for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 2)
            //     {
            //         scrollRect.normalizedPosition = Vector2.Lerp(lastPosition, targetLocation, Mathf.SmoothStep(0, 1, t));
            //         yield return null;
            //     }
            // }

            scrollRect.normalizedPosition = targetLocation;
            _initialized = true;
            yield return null;
        }
    }
}