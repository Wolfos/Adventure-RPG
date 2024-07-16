using System;
using System.Collections.Generic;
using Player;
using UnityEngine;

namespace UI
{
    public class Compass : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private float compassDegreesShown = 100;
        [SerializeField] private List<CompassElement> _compassElements;

        private void Update()
        {
            var cameraHeading = PlayerCamera.LookHeading; // Camera heading in degrees

            foreach (var element in _compassElements)
            {
                float angleDifference = Mathf.DeltaAngle(cameraHeading, element.heading);

                float compassWidth = rectTransform.rect.width;
                float normalizedPosition = angleDifference / compassDegreesShown;

                float elementPositionX = normalizedPosition * compassWidth;
                element.rectTransform.anchoredPosition = new Vector2(elementPositionX, element.rectTransform.anchoredPosition.y);
            }
        }
    }
}