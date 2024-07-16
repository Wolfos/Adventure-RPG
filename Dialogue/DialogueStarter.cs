using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Interface;
using UI;

namespace Dialogue
{

    public class DialogueStarter : MonoBehaviour, IInteractable
    {
        public string startNode;
        public string friendlyName = "talk";
        public NPC dialogueCharacter;
        
        public void OnCanInteract(CharacterBase character)
        {
            // TODO: Localize
            Tooltip.Activate(friendlyName);
        }

        public void OnInteract(CharacterBase character)
        {
            DialogueWindow.SetData(startNode, dialogueCharacter, character);
            WindowManager.Open<DialogueWindow>();
        }
		
        public void OnEndInteract(CharacterBase character)
        {
            Tooltip.DeActivate();
        }

        private void OnDestroy()
        {
            Tooltip.DeActivate();
        }
    }
}