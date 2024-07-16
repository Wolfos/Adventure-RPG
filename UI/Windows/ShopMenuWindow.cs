using System;
using Items;
using Player;
using UnityEngine;

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
            shopInventoryView.Container = _itemContainer;
            shopInventoryView.OtherContainer = PlayerCharacter.GetInventory();
            playerInventoryView.Container = PlayerCharacter.GetInventory();
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