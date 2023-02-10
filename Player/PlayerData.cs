using System;
using System.Collections.Generic;
using Data;
using Character;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerData : SaveableObject
{
	[FormerlySerializedAs("player")] [SerializeField] private PlayerCharacter playerCharacter;
	
	public override void Load(string json)
	{
		var data = JsonUtility.FromJson<CharacterData>(json);
		CharacterPool.Register(data.characterId, playerCharacter);
		playerCharacter.data = data;
		playerCharacter.characterController.enabled = false;
		transform.position = data.position;
		playerCharacter.graphic.rotation = data.rotation;
		playerCharacter.SetHealth(data.health);

		for (int i = 0; i < playerCharacter.inventory.slots; i++)
		{
			playerCharacter.inventory.DestroyItem(i);
		}
			
		for (int i = 0; i < data.items.Count; i++)
		{
			for (int j = 0; j < data.quantities[i]; j++)
			{
				playerCharacter.inventory.AddItem(data.items[i]);
			}

			playerCharacter.inventory.items[i].IsEquipped = data.equipped[i];
		}
		
		
		playerCharacter.GetComponent<CharacterEquipment>().CheckEquipment();	
		playerCharacter.characterController.enabled = true;
	}
		
	public override string Save()
	{
		var items = new List<int>();
		var equipped = new List<bool>();
		var quantities = new List<int>();
		foreach (var item in playerCharacter.inventory.items)
		{
			if (item != null)
			{
				items.Add(item.id);
				equipped.Add(item.IsEquipped);
				quantities.Add(item.Quantity);
			}
		}

		var data = playerCharacter.data;
		data.position = transform.position;
		data.rotation = playerCharacter.graphic.rotation;
		data.items = items;
		data.equipped = equipped;
		data.quantities = quantities;

		return JsonUtility.ToJson(data);
	}
}
