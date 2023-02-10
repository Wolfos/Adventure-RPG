using System;
using Items;
using Player;
using UnityEngine;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public class ShopMenuWindow : Window
    {
        [SerializeField] private InventoryView shopInventoryView;
        [SerializeField] private InventoryView playerInventoryView;

        private Action _onShoppingDoneCallback;

        private static Container _itemContainer;
        public static Action OnShoppingDone;

        private void OnEnable()
        {
            var player = SystemContainer.GetSystem<PlayerCharacter>();
            shopInventoryView.container = _itemContainer;
            shopInventoryView.otherContainer = player.inventory;
            playerInventoryView.container = player.inventory;
            playerInventoryView.otherContainer = _itemContainer;
        }

        public static void SetData(Container itemContainer)
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