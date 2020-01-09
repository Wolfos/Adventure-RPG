using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NPC;
using UI;

namespace Dialogue
{

    public class DialogueStarter : MonoBehaviour
    {
        [SerializeField] private string conversationAsset;
        [SerializeField] private string friendlyName;
        [SerializeField] private float tooltipOffset;
        
        private void OnCanInteract()
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(transform.position + transform.up * tooltipOffset);
            Tooltip.Activate(friendlyName, screenPosition);
        }

        private void OnInteract(Character character)
        {
            DialoguePanel.Activate(conversationAsset);
        }
		
        private void OnEndInteract()
        {
            Tooltip.DeActivate();
        }
    }
}