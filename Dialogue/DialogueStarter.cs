using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
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
            Tooltip.Activate(friendlyName, transform, transform.up * tooltipOffset);
        }

        private void OnInteract(CharacterBase character)
        {
            DialoguePanel.Activate(conversationAsset);
        }
		
        private void OnEndInteract()
        {
            Tooltip.DeActivate();
        }
    }
}