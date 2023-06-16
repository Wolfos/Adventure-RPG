using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using Data;
using Interface;
using Items;
using Models;
using Newtonsoft.Json;
using UI;
using UnityEngine.Scripting;
using WolfRPG.Core;
using WolfRPG.Inventory;

public class Chest : SaveableObject, IInteractable
{
	private bool _isOpen;
	[SerializeField] private Animator animator;
	[SerializeField] private RPGObjectReference[] items;

	private ItemContainer _container = new();
	
	private class ChestSaveData
	{
		public bool IsOpen { get; set; }
		// GUID, item quantity
		public List<Tuple<string, int>> ItemsAndQuantities { get; set; }
	}

	protected override void Start()
	{
		foreach (var item in items)
		{
			_container.AddItem(item.GetObject());
		}
		
		base.Start();
	}
	
	public override void Load(string json)
	{
		_container.Clear();
		
		var saveData = JsonConvert.DeserializeObject<ChestSaveData>(json);
		foreach (var tuple in saveData.ItemsAndQuantities)
		{
			_container.AddItem(tuple.Item1, tuple.Item2);
		}

		_isOpen = saveData.IsOpen;
	}

	public override string Save()
	{
		var saveData = new ChestSaveData
		{
			IsOpen = _isOpen,
			ItemsAndQuantities = new()
		};
		
		for (int i = 0; i < _container.ItemCount; i++)
		{
			var guid = _container.GetItemBySlot(i).RpgObject.Guid;
			var quantity = _container.GetQuantityFromSlot(i);
			saveData.ItemsAndQuantities.Add(new(guid,quantity));
		}

		return JsonConvert.SerializeObject(saveData);
	}

	

	[Preserve]
	public void OnCanInteract(CharacterBase character)
	{
		Tooltip.Activate(_isOpen ? "Close" : "Open", transform, Vector3.zero);
	}
	
	[Preserve]
	public void OnInteract(CharacterBase character)
	{
		ItemContainerWindow.SetData(_container);
		WindowManager.Open<ItemContainerWindow>();

		

		_isOpen = !_isOpen;
		
		if (animator == null)
		{
			return;
		}
		
		if (_isOpen)
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
