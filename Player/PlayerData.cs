using System;
using System.Collections.Generic;
using Data;
using Character;
using Newtonsoft.Json;
using Player;
using UnityEngine;
using UnityEngine.Serialization;
using WolfRPG.Character;
using Attribute = WolfRPG.Core.Statistics.Attribute;

public class PlayerData : SaveableObject
{
	[FormerlySerializedAs("player")] [SerializeField] private PlayerCharacter playerCharacter;

	private class PlayerSaveData
	{
		public CharacterData CharacterData { get; set; }
		// GUID, item quantity
		public List<Tuple<string, int>> ItemsAndQuantities { get; set; }
	}
	
	public override void Load(string json)
	{
		var saveData = JsonConvert.DeserializeObject<PlayerSaveData>(json, WolfRPG.Core.Settings.JsonSerializerSettings);
		var data = saveData.CharacterData;
		
		CharacterPool.Register(data.CharacterComponent.CharacterId, playerCharacter);
		playerCharacter.Data = data;
		playerCharacter.characterController.enabled = false;
		transform.position = data.CharacterComponent.Position;
		playerCharacter.graphic.rotation = data.CharacterComponent.Rotation;
		playerCharacter.SetHealth(data.GetAttributeValue(Attribute.Health));

		if (saveData.ItemsAndQuantities != null)
		{
			foreach (var tuple in saveData.ItemsAndQuantities)
			{
				playerCharacter.Inventory.AddItem(tuple.Item1, tuple.Item2);
			}
		}


		playerCharacter.GetComponent<CharacterEquipment>().CheckEquipment();	
		playerCharacter.characterController.enabled = true;
		
		playerCharacter.RegisterCallbacks();
	}
		
	public override string Save()
	{ 
		var characterData = playerCharacter.Data;
		var playerTransform = transform;
		characterData.CharacterComponent.Position = playerTransform.position;
		characterData.CharacterComponent.Rotation = playerTransform.rotation;

		var saveData = new PlayerSaveData
		{
			CharacterData = characterData,
			ItemsAndQuantities = new()
		};

		for (int i = 0; i < playerCharacter.Inventory.ItemCount; i++)
		{
			var guid = playerCharacter.Inventory.GetItemBySlot(i).RpgObject.Guid;
			var quantity = playerCharacter.Inventory.GetQuantityFromSlot(i);
			saveData.ItemsAndQuantities.Add(new(guid,quantity));
		}

		return JsonConvert.SerializeObject(saveData, WolfRPG.Core.Settings.JsonSerializerSettings);
	}
}
