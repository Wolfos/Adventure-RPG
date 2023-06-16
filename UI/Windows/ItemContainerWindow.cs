using System;
using System.Collections;
using System.Collections.Generic;
using Items;
using Player;
using UnityEngine;
using Utility;
using WolfRPG.Inventory;

namespace UI
{
    public class ItemContainerWindow : Window
    {
        [SerializeField] private InventoryView itemInventoryView;
        [SerializeField] private InventoryView playerInventoryView;
        private static ItemContainer _itemContainer;

        private void OnEnable()
        {
            var player = SystemContainer.GetSystem<PlayerCharacter>();
            itemInventoryView.Container = _itemContainer;
            itemInventoryView.OtherContainer = player.Inventory;
            playerInventoryView.Container = player.Inventory;
            playerInventoryView.OtherContainer = _itemContainer;
        }

        public static void SetData(ItemContainer itemContainer)
        {
            _itemContainer = itemContainer;
        }
    }
}