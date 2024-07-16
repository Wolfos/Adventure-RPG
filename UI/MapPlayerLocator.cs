using Player;
using UnityEngine;

namespace UI
{
    public class MapPlayerLocator : MonoBehaviour
    {
        // [SerializeField] private Vector2 topLeft; // Top left of the map in world space, the Y component is actually Z
        // [SerializeField] private Vector2 bottomRight; // Bottom right of the map in world space, the Y component is actually Z
        // [SerializeField] private Vector2 mapSize; // Map size in pixels
        //
        // private void OnEnable()
        // {
        //     var playerPosition = PlayerCharacter.GetPosition(); // Vector3 in world space
        //     
        //     // Set the position of this object's RectTransform to the correct spot on the map, based on the player position
        //     Vector2 playerMapPosition = WorldToMapPosition(playerPosition);
        //
        //     // Set the RectTransform's position
        //     var rectTransform = transform as RectTransform;
        //     rectTransform.anchoredPosition = playerMapPosition;
        // }
        //
        // private Vector2 WorldToMapPosition(Vector3 worldPosition)
        // {
        //     // Calculate the relative position within the world space map boundaries
        //     var relativeX = (worldPosition.x - topLeft.x) / (bottomRight.x - topLeft.x);
        //     var relativeY = (worldPosition.z - topLeft.y) / (bottomRight.y - topLeft.y); // Use z for Y component in world space
        //
        //     // Convert relative position to map pixel position
        //     var mapX = relativeX * mapSize.x;
        //     var mapY = relativeY * mapSize.y;
        //
        //     return new (mapX, -mapY);
        // }
    }
}