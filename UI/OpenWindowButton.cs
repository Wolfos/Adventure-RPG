using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace UI
{
    public enum Windows
    {
        Dialogue, ItemContainer, PauseMenu, PlayerMenu, SettingsMenu, ShopMenu
    }
    public class OpenWindowButton : MonoBehaviour
    {
        [SerializeField] private Windows window;
        
        [Preserve]
        public void Open()
        {
            switch (window)
            {
                case Windows.Dialogue:
                    WindowManager.Open<DialogueWindow>();
                    break;
                case Windows.ItemContainer:
                    WindowManager.Open<ItemContainerWindow>();
                    break;
                case Windows.PauseMenu:
                    WindowManager.Open<PauseMenuWindow>();
                    break;
                case Windows.PlayerMenu:
                    WindowManager.Open<PlayerMenuWindow>();
                    break;
                case Windows.SettingsMenu:
                    WindowManager.Open<SettingsMenuWindow>();
                    break;
                case Windows.ShopMenu:
                    WindowManager.Open<ShopMenuWindow>();
                    break;
            }
        }
        
        [Preserve]
        public void Close()
        {
            switch (window)
            {
                case Windows.Dialogue:
                    WindowManager.Close<DialogueWindow>();
                    break;
                case Windows.ItemContainer:
                    WindowManager.Close<ItemContainerWindow>();
                    break;
                case Windows.PauseMenu:
                    WindowManager.Close<PauseMenuWindow>();
                    break;
                case Windows.PlayerMenu:
                    WindowManager.Close<PlayerMenuWindow>();
                    break;
                case Windows.SettingsMenu:
                    WindowManager.Close<SettingsMenuWindow>();
                    break;
                case Windows.ShopMenu:
                    WindowManager.Close<ShopMenuWindow>();
                    break;
            }
        }
    }
}