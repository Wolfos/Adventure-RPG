using System.Collections.Generic;
using Character;
using UnityEngine;
using Data;
using UI;
using Utility;

public class Chest : SaveableObject
{
	// TODO: This should be saved	
	private bool isOpen;
	[SerializeField] private List<int> items;
	[SerializeField] private List<int> quantity;
	[SerializeField] private Animator animator;
	[SerializeField] private GameObject itemAnimationPrefab;
	[SerializeField] private Transform itemSpawn;

	public override string Save()
	{
		return "";
	}

	public override void Load(string json)
	{
	}

	private void OnCanInteract()
	{
		Tooltip.Activate(isOpen ? "Close" : "Open", transform, Vector3.zero);
	}

	private void OnInteract(CharacterBase character)
	{
		int i = 0;
		foreach (int item in items)
		{
			for (int j = 0; j < quantity[i]; j++)
			{
				character.inventory.AddItem(item);
			}

			GameObject itemAnim = Instantiate(itemAnimationPrefab, itemSpawn.position, itemAnimationPrefab.transform.rotation);
			itemAnim.GetComponent<SpriteRenderer>().sprite = Database.GetDatabase<ItemDatabase>().items[item].icon;
			i++;
		}

		items.Clear();

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

	private void OnEndInteract()
	{
		Tooltip.DeActivate();
	}
}
