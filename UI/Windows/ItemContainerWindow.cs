using Character;
using Player;
using UnityEngine;
using ItemContainer = Items.ItemContainer;

namespace UI
{
    public class ItemContainerWindow : Window
    {
        [SerializeField] private InventoryView itemInventoryView;
        [SerializeField] private InventoryView playerInventoryView;
        private static ItemContainer _itemContainer;
        private static CharacterEquipment _equipment;

        private void OnEnable()
        {
            itemInventoryView.Container = _itemContainer;
            itemInventoryView.Equipment = _equipment;
            itemInventoryView.OtherContainer = PlayerCharacter.GetInventory();
            playerInventoryView.Container = PlayerCharacter.GetInventory();
            playerInventoryView.Equipment = PlayerCharacter.GetEquipment();
            playerInventoryView.OtherContainer = _itemContainer;
        }

        public static void SetData(ItemContainer itemContainer, CharacterEquipment equipment)
        {
            _itemContainer = itemContainer;
            _equipment = equipment;
        }
    }
}