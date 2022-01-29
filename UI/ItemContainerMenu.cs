using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using Player;
using UnityEngine;
using Utility;

namespace UI
{
    public class ItemContainerMenu : MonoBehaviour
    {
        private static ItemContainerMenu _instance;
        public static bool IsActive;
        
        [SerializeField] private Inventory itemInventory;
        [SerializeField] private Inventory playerInventory;
        
        public static void Enable(Container itemContainer)
        {
            _instance.itemInventory.container = itemContainer;
            var player = SystemContainer.GetSystem<PlayerCharacter>();
            _instance.itemInventory.otherContainer = player.inventory;
            _instance.playerInventory.otherContainer = itemContainer;
            var gameObject = _instance.gameObject;
            gameObject.SetActive(true);
            IsActive = gameObject.activeSelf;
            PlayerCharacter.SetInputActive(false);
            Time.timeScale = 0;
        }

        public static void Disable()
        {
            var gameObject = _instance.gameObject;
            gameObject.SetActive(false);
            IsActive = gameObject.activeSelf;
            PlayerCharacter.SetInputActive(true);
            Time.timeScale = 1;
        }

        private void Update()
        {
            if (InputMapper.InteractionButton())
            {
                Disable();
            }
        }

        private void Awake()
        {
            _instance = this;
            gameObject.SetActive(false);
        }
    }
}