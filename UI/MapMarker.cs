using Player;
using UnityEngine;
using UnityEngine.UI;
using World;

namespace UI
{
    public class MapMarker : MonoBehaviour
    {
        [SerializeField] private Image image;
        [SerializeField] private RectTransform rectTransform;

        private TeleportDestinations.Destination _destination;
        public void Initialize(TeleportDestinations.Destination destination, Vector2 anchoredPosition)
        {
	        image.sprite = destination.MapIcon;
            rectTransform.anchoredPosition = anchoredPosition;
            _destination = destination;
        }

        public void OnClick()
        {   
            WindowManager.Close<PlayerMenuWindow>();
            PlayerCharacter.Teleport(_destination);
        }
    }
}