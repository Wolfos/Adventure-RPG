using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using Player;
using UnityEngine;
using Utility;

namespace UI
{
    public class ItemContainerWindow : Window
    {
        [SerializeField] private InventoryView itemInventoryView;
        [SerializeField] private InventoryView playerInventoryView;
        private static Container _itemContainer;

        private void OnEnable()
        {
            var player = SystemContainer.GetSystem<PlayerCharacter>();
            itemInventoryView.container = _itemContainer;
            itemInventoryView.otherContainer = player.inventory;
            playerInventoryView.container = player.inventory;
            playerInventoryView.otherContainer = _itemContainer;
        }

        public static void SetData(Container itemContainer)
        {
            _itemContainer = itemContainer;
        }
    }
}