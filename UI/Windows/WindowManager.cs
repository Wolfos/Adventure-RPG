using System;
using System.Linq;
using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace UI
{
	public class WindowManager: MonoBehaviour
	{
		[SerializeField] private Window[] windows;
		private static WindowManager _instance;

		private void Awake()
		{
			_instance = this;
		}

		private void OnEnable()
		{
			EventManager.OnPlayerMenu += OnPlayerMenu;
			EventManager.OnPauseMenu += OnPauseMenu;
		}

		private void OnDisable()
		{
			EventManager.OnPlayerMenu -= OnPlayerMenu;
			EventManager.OnPauseMenu -= OnPauseMenu;
		}

		private void OnPlayerMenu(InputAction.CallbackContext context)
		{
			if (context.canceled)
			{
				Toggle<PlayerMenuWindow>();
			}
		}
		
		private void OnPauseMenu(InputAction.CallbackContext context)
		{
			if (context.canceled)
			{
				var activeWindow = GetActiveWindow();
				if (activeWindow != null)
				{
					Close(activeWindow, false);
				}
				else
				{
					Toggle<PauseMenuWindow>();
				}
			}
		}

		public static void Toggle<T>() where T : Window
		{
			var window = _instance.windows.First(w => w is T);
			
			if(window.Active) Close(window);
			else Open(window);
		}

		public static void Open<T>() where T : Window
		{
			var window = _instance.windows.First(w => w is T);
			Open(window);
		}

		private static void Open(Window window)
		{
			foreach (var w in _instance.windows)
			{
				w.SetSortingOrder(1);
			}
			window.SetSortingOrder(2);
			window.Open();
			if (window.pauseWhenOpen)
			{
				Time.timeScale = 0;
				PlayerControls.SetInputActive(false);
			}
		}
		
		public static void Close<T>() where T : Window
		{
			var window = _instance.windows.First(w => w is T);
			Close(window);
		}

		public static void Close(Window window, bool force = true)
		{
			if (force || window.closeAble)
			{
				window.Close();
			}

			if (IsAnyWindowOpen() == false)
			{
				Time.timeScale = 1;
				PlayerControls.SetInputActive(true);
			}
		}

		public static bool IsAnyWindowOpen(bool pause = true)
		{
			foreach (var window in _instance.windows)
			{
				if (window.Active && (!pause || window.pauseWhenOpen)) return true;
			}

			return false;
		}

		public static Window GetActiveWindow()
		{
			var highestOrder = 0;
			Window activeWindow = null;
			foreach (var window in _instance.windows)
			{
				var sortingOrder = window.GetSortingOrder();
				if (window.Active && sortingOrder > highestOrder)
				{
					highestOrder = sortingOrder;
					activeWindow = window;
				}
			}

			return activeWindow;
		}
	}
}