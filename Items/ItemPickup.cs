using System;
using Character;
using Data;
using Interface;
using UI;
using UnityEngine;
using WolfRPG.Core;
using WolfRPG.Core.Localization;
using Utility;

namespace Items
{
    public class ItemPickup: SaveableObject, IInteractable
    {
        private class ItemPickupData : ISaveData
        {
            public bool WasPickedUp { get; set; }
        }

        private ItemPickupData _data;
        [SerializeField] private LocalizedString itemName;
        [SerializeField] public ItemData itemObject;


        private void Start()
        {
            if (string.IsNullOrEmpty(id) == false) // Dropped objects have their save data stored differently
            {
                if (SaveGameManager.HasData(id))
                {
                    _data = SaveGameManager.GetData(id) as ItemPickupData;
                }
                else
                {
                    _data = new();
                    SaveGameManager.Register(id, _data);
                }
            }
        }

        public void OnCanInteract(CharacterBase characterBase)
        {
            if (_data.WasPickedUp) return; // Unsure why but it calls OnCanInteract again after destroying
            
            Tooltip.Activate(itemName.Get());
        }

        public void OnInteract(CharacterBase characterBase)
        {
            characterBase.Inventory.AddItem(itemObject);
            OnEndInteract(characterBase);
            Destroy(gameObject);
            _data.WasPickedUp = true;
        }

        public void OnEndInteract(CharacterBase characterBase)
        {
            Tooltip.DeActivate();
        }
    }
}