using System;
using System.Collections.Generic;
using Data;
using Character;
using Player;
using UnityEngine;

public class PlayerData : SaveableObject
{
	[SerializeField] private Player.Player player;
	
	public override void Load(string json)
	{
		var data = JsonUtility.FromJson<CharacterData>(json);
		CharacterPool.Register(data.characterId, player);
		player.data = data;
		player.characterController.enabled = false;
		transform.position = data.position;
		player.graphic.rotation = data.rotation;

		for (int i = 0; i < player.inventory.slots; i++)
		{
			player.inventory.DestroyItem(i);
		}
			
		for (int i = 0; i < data.items.Count; i++)
		{
			for (int j = 0; j < data.quantities[i]; j++)
			{
				player.inventory.AddItem(data.items[i]);
			}

			player.inventory.items[i].Equipped = data.equipped[i];
		}
		
		
		player.GetComponent<CharacterEquipment>().CheckEquipment();	
		player.characterController.enabled = true;
	}
		
	public override string Save()
	{
		var items = new List<int>();
		var equipped = new List<bool>();
		var quantities = new List<int>();
		foreach (var item in player.inventory.items)
		{
			if (item != null)
			{
				items.Add(item.id);
				equipped.Add(item.Equipped);
				quantities.Add(item.quantity);
			}
		}

		var data = player.data;
		data.position = transform.position;
		data.rotation = player.graphic.rotation;
		data.items = items;
		data.equipped = equipped;
		data.quantities = quantities;

		return JsonUtility.ToJson(data);
	}
}
