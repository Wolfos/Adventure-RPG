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
            itemInventoryView.Container = _itemContainer;
            itemInventoryView.OtherContainer = PlayerCharacter.GetInventory();
            playerInventoryView.Container = PlayerCharacter.GetInventory();
            playerInventoryView.OtherContainer = _itemContainer;
        }

        public static void SetData(ItemContainer itemContainer)
        {
            _itemContainer = itemContainer;
        }
    }
}