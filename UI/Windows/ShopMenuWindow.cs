using System;
using Items;
using Player;
using UnityEngine;
using Utility;
using WolfRPG.Inventory;

namespace UI
{
    public class ShopMenuWindow : Window
    {
        [SerializeField] private InventoryView shopInventoryView;
        [SerializeField] private InventoryView playerInventoryView;

        private Action _onShoppingDoneCallback;

        private static ItemContainer _itemContainer;
        public static Action OnShoppingDone;

        private void OnEnable()
        {
            var player = SystemContainer.GetSystem<PlayerCharacter>();
            shopInventoryView.Container = _itemContainer;
            shopInventoryView.OtherContainer = player.Inventory;
            playerInventoryView.Container = player.Inventory;
            playerInventoryView.OtherContainer = _itemContainer;
        }

        public static void SetData(ItemContainer itemContainer)
        {
            _itemContainer = itemContainer;
        }

        private void OnDisable()
        {
            OnShoppingDone?.Invoke();
        }

        public void CloseButtonPressed()
        {
            WindowManager.Close(this);
        }
    }
}