using System;
using System.Collections.Generic;
using Character;
using UnityEngine;
using Data;
using Interface;
using UI;
using UnityEngine.Scripting;
using ItemContainer = Items.ItemContainer;
using ItemData = Items.ItemData;

public class Chest : SaveableObject, IInteractable
{
	private ChestSaveData _data;
	[SerializeField] private Animator animator;
	[SerializeField] private ItemData[] items;

	private ItemContainer _container;
	
	private class ChestSaveData: ISaveData
	{
		public bool IsOpen { get; set; }
		// GUID, item quantity
		public List<Tuple<Guid, int>> ItemsAndQuantities { get; set; } = new();
	}

	private void Start()
	{
		_container = new();
		
		if (SaveGameManager.HasData(id))
		{
			_data = SaveGameManager.GetData(id) as ChestSaveData;
			foreach (var tuple in _data.ItemsAndQuantities)
			{
				_container.AddItem(tuple.Item1, tuple.Item2);
			}
		}
		else
		{
			_data = new();
			foreach (var item in items)
			{
				_container.AddItem(item);
			}
			ContentsChanged();
		}
		
		_container.OnContentsChanged += ContentsChanged;
	}

	private void OnDestroy()
	{
		_container.OnContentsChanged -= ContentsChanged;
	}

	private void ContentsChanged()
	{
		_data.ItemsAndQuantities.Clear();
		for (int i = 0; i < _container.ItemCount; i++)
		{
			var guid = _container.GetItemBySlot(i).Guid;
			var quantity = _container.GetQuantityFromSlot(i);
			_data.ItemsAndQuantities.Add(new(guid,quantity));
		}
	}

	[Preserve]
	public void OnCanInteract(CharacterBase character)
	{
		// TODO: Localize
		Tooltip.Activate(_data.IsOpen ? "Close" : "Open");
	}
	
	[Preserve]
	public void OnInteract(CharacterBase character)
	{
		ItemContainerWindow.SetData(_container, null);
		WindowManager.Open<ItemContainerWindow>();

		_data.IsOpen = !_data.IsOpen;
		
		if (animator == null)
		{
			return;
		}
		
		if (_data.IsOpen)
		{
			animator.SetTrigger("Open");
		}
		else
		{
			animator.SetTrigger("Close");
		}
	}

	[Preserve]
	public void OnEndInteract(CharacterBase character)
	{
		Tooltip.DeActivate();
	}
}
