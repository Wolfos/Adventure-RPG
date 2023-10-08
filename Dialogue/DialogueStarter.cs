﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Character;
using Interface;
using UI;

namespace Dialogue
{

    public class DialogueStarter : MonoBehaviour, IInteractable
    {
        public DialogueNodeGraph dialogueAsset;
        public string friendlyName = "test";
        [SerializeField] private float tooltipOffset = 3;
        
        public void OnCanInteract(CharacterBase character)
        {
            Tooltip.Activate(friendlyName, transform, transform.up * tooltipOffset);
        }

        public void OnInteract(CharacterBase character)
        {
            DialogueWindow.SetData(dialogueAsset, GetComponent<NPC>(), character);
            WindowManager.Open<DialogueWindow>();
        }
		
        public void OnEndInteract(CharacterBase character)
        {
            Tooltip.DeActivate();
        }
    }
}