using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    public class ItemSortButton : MonoBehaviour, ISelectable
    {
        [SerializeField] private InventoryView inventoryView;
        [SerializeField] private ItemSort sortAscending, sortDescending;
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private Color baseColor;
        [SerializeField] private Color selectedColor;

        public Button button; 
        
        private bool _toggled;
        private string _baseText;

        private void Awake()
        {
            _baseText = text.text; // TODO: Localization
        }

        public void SetSortType(ItemSort activeSort)
        {
            if (activeSort == sortAscending)
            {
                text.text = _baseText + "\u25b2";
                _toggled = true;
            }
            else if (activeSort == sortDescending)
            {
                text.text = _baseText + "\u25bc";
                _toggled = false;
            }
            else
            {
                _toggled = false;
                text.text = _baseText;
            }
        }

        public void Clicked()
        {
            inventoryView.SortItems(_toggled ? sortDescending : sortAscending);
        }
        
        public void OnSelect()
        {
            text.color = selectedColor;
        }

        public void OnDeselect()
        {
            text.color = baseColor;
        }

        public void Confirm()
        {
            Clicked();
        }
    }
}