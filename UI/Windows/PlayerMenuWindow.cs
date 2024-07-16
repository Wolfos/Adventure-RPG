using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Utility;

namespace UI
{
    public enum PlayerMenuTab
    {
        NONE, Inventory, Character, Quests, Map
    }
    public class PlayerMenuWindow : Window
    {
        [SerializeField] private Button[] topPanelButtons;
        private int _currentButton;

        [SerializeField] private GameObject inventory;
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject quests;
        [SerializeField] private GameObject map;

        [SerializeField] private PlayerMenuTabComponent characterButton, inventoryButton, questsbutton, mapButton;

        private PlayerMenuTab _activeTab;

        private void OnEnable()
        { 
            SetActiveButtonColor();
            EventManager.OnMenuLeft += OnMenuLeft;
            EventManager.OnMenuRight += OnMenuRight;

            _currentButton = 1;
            ChangeTab(PlayerMenuTab.Inventory);
        }

        private void OnDisable()
        {
            EventManager.OnMenuLeft -= OnMenuLeft;
            EventManager.OnMenuRight -= OnMenuRight;
        }

        private void DisableAll()
        {
            inventory.SetActive(false);
            character.SetActive(false);
            quests.SetActive(false);
            map.SetActive(false);
            
            characterButton.SetInactive();
            inventoryButton.SetInactive();
            questsbutton.SetInactive();
            mapButton.SetInactive();
        }

        public void CharacterTab()
        {
            ChangeTab(PlayerMenuTab.Character);
        }

        public void InventoryTab()
        {
            ChangeTab(PlayerMenuTab.Inventory);
        }

        public void QuestsTab()
        {
            ChangeTab(PlayerMenuTab.Quests);   
        }

        public void MapTab()
        {
            ChangeTab(PlayerMenuTab.Map);   
        }
        
        public void ChangeTab(PlayerMenuTab tab)
        {
            if (tab == _activeTab) return;
            _activeTab = tab;
            DisableAll();
            
            switch (tab)
            {
                case PlayerMenuTab.Inventory:
                    inventory.SetActive(true);
                    inventoryButton.SetActive();
                    break;
                case PlayerMenuTab.Character:
                    character.SetActive(true);
                    characterButton.SetActive();
                    break;
                case PlayerMenuTab.Quests:
                    quests.SetActive(true);
                    questsbutton.SetActive();
                    break;
                case PlayerMenuTab.Map:
                    map.SetActive(true);
                    mapButton.SetActive();
                    break;
            }
        }
        

        private void SetActiveButtonColor()
        {
            if (!InputMapper.UsingController) return;
            
            var i = 0;
            foreach (var button in topPanelButtons)
            {
                if (i == _currentButton)
                {
                    var colors = button.colors;
                    colors.normalColor = button.colors.highlightedColor;
                    button.colors = colors;
                }
                else
                {
                    var colors = button.colors;
                    colors.normalColor = button.colors.disabledColor;
                    button.colors = colors;
                }
                i++;
            }
        }

        private void OnMenuLeft(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
            {
                _currentButton--;
                if (_currentButton < 0) _currentButton = topPanelButtons.Length - 1;

                topPanelButtons[_currentButton].onClick.Invoke();
                SetActiveButtonColor();
            }
        }
        
        private void OnMenuRight(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Canceled)
            {
                _currentButton++;
                if (_currentButton >= topPanelButtons.Length) _currentButton = 0;

                topPanelButtons[_currentButton].onClick.Invoke();
                SetActiveButtonColor();
            }
        }
    }
}