using System;
using System.Collections.Generic;
using System.Linq;
using Character;
using UnityEngine;
using Data;
using Interface;
using Items;
using Models;
using UI;
using UnityEngine.Scripting;

public class Chest : SaveableObject, IInteractable
{
	// TODO: This should be saved
	private bool isOpen;
	[SerializeField] private Animator animator;
	[SerializeField] private Container container;


	public override string Save()
	{
		var data = new ContainerData();
		foreach (var item in container.items.Where(item => item != null))
		{
			data.itemIds.Add(item.id);
			data.itemQuantities.Add(item.Quantity);
		}
		var json = JsonUtility.ToJson(data);
		return json;
	}

	public override void Load(string json)
	{
		container.Clear();
		var data = JsonUtility.FromJson<ContainerData>(json);
		for(var i = 0; i < data.itemIds.Count; i++)
		{
			for (var ii = 0; ii < data.itemQuantities[i]; ii++)
			{
				container.AddItem(data.itemIds[i]);
			}
		}
	}

	[Preserve]
	public void OnCanInteract(CharacterBase character)
	{
		Tooltip.Activate(isOpen ? "Close" : "Open", transform, Vector3.zero);
	}
	
	[Preserve]
	public void OnInteract(CharacterBase character)
	{
		ItemContainerWindow.SetData(container);
		WindowManager.Open<ItemContainerWindow>();

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
	public void OnEndInteract(CharacterBase character)
	{
		Tooltip.DeActivate();
	}
}
