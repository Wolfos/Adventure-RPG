using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class PlayerMenuTabComponent : MonoBehaviour
    {
        [SerializeField] private Color inactiveColor, activeColor, disabledColor;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Button button;
        public bool IsEnabled { get; private set; }
        private bool _isActive;


        public void SetActive()
        {
            text.color = button.interactable ? activeColor : disabledColor;
            _isActive = true;
        }

        public void SetInactive()
        {
            text.color = button.interactable ? inactiveColor : disabledColor;
            _isActive = false;
        }

        public void SetEnabled(bool enabled)
        {
            IsEnabled = enabled;
            button.interactable = enabled;
            text.color = enabled ? (_isActive ? activeColor : inactiveColor) : disabledColor;
        }
    }
}