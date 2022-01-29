using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using Data;
using Items;
using UI;
using UnityEngine.Scripting;

public class Chest : SaveableObject
{
	// TODO: This should be saved
	private bool isOpen;
	[SerializeField] private Animator animator;
	[SerializeField] private Container container;

	[Serializable]
	private class ChestData
	{
		public List<int> itemIds;
		public List<int> itemQuantities;

		public ChestData()
		{
			itemIds = new List<int>();
			itemQuantities = new List<int>();
		}
	}


	public override string Save()
	{
		var data = new ChestData();
		foreach (var item in container.items.Where(item => item != null))
		{
			data.itemIds.Add(item.id);
			data.itemQuantities.Add(item.quantity);
		}
		var json = JsonUtility.ToJson(data);
		return json;
	}

	public override void Load(string json)
	{
		container.Clear();
		var data = JsonUtility.FromJson<ChestData>(json);
		for(var i = 0; i < data.itemIds.Count; i++)
		{
			for (var ii = 0; ii < data.itemQuantities[i]; ii++)
			{
				container.AddItem(data.itemIds[i]);
			}
		}
	}

	[Preserve]
	private void OnCanInteract()
	{
		Tooltip.Activate(isOpen ? "Close" : "Open", transform, Vector3.zero);
	}
	
	[Preserve]
	private void OnInteract(CharacterBase character)
	{
		ItemContainerMenu.Enable(container);

		if (isOpen)
		{
			animator.SetTrigger("Close");
		}
		else
		{
			animator.SetTrigger("Open");
		}

		isOpen = !isOpen;
	}

	[Preserve]
	private void OnEndInteract()
	{
		Tooltip.DeActivate();
	}
}
