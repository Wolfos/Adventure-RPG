using System;
using System.Collections.Generic;
using Data;
using Newtonsoft.Json;
using Player;
using WolfRPG.Character;
using WolfRPG.Core.Quests;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Character
{
	public static class CharacterSaveUtility
	{
		private class CharacterSaveData
		{
			public CharacterData CharacterData { get; set; }
			public int Money { get; set; }
			// GUID, item quantity
			public List<Tuple<string, int>> ItemsAndQuantities { get; set; }
			public List<Tuple<string, int>> ItemsAndQuantitiesShop { get; set; }
			public Dictionary<string, QuestProgress> QuestProgress { get; set; }
		}

		public static void Load(string json, CharacterBase character)
		{
			var saveData = JsonConvert.DeserializeObject<CharacterSaveData>(json, WolfRPG.Core.Settings.JsonSerializerSettings);
			var data = saveData.CharacterData;
		
			CharacterPool.Register(data.CharacterComponent.CharacterId, character);
			character.Data = data;
			
			character.transform.position = data.CharacterComponent.Position;
			character.graphic.rotation = data.CharacterComponent.Rotation;
			character.SetHealth(data.GetAttributeValue(Attribute.Health));

			character.Inventory.Money = saveData.Money;

			if (saveData.ItemsAndQuantities != null)
			{
				foreach (var tuple in saveData.ItemsAndQuantities)
				{
					character.Inventory.AddItem(tuple.Item1, tuple.Item2);
				}
			}

			if (saveData.ItemsAndQuantitiesShop != null && character is NPC {ShopInventory: not null} npc)
			{
				npc.ShopInventory.Clear();
				foreach (var tuple in saveData.ItemsAndQuantities)
				{
					npc.ShopInventory.AddItem(tuple.Item1, tuple.Item2);
				}
				
			}
			
			if (character is PlayerCharacter player)
			{
				player.QuestProgress = saveData.QuestProgress;
			}



			character.GetComponent<CharacterEquipment>().CheckEquipment();
		}
		
		public static string GetSaveData(CharacterBase character)
		{
			var characterData = character.Data;
			var transform = character.transform;
				
			characterData.CharacterComponent.Position = transform.position;
			characterData.CharacterComponent.Rotation = transform.rotation;
	
			var saveData = new CharacterSaveData
			{
				CharacterData = characterData,
				ItemsAndQuantities = new(),
				ItemsAndQuantitiesShop = new(),
				Money = character.Inventory.Money
			};

			// Character inventory
			for (int i = 0; i < character.Inventory.ItemCount; i++)
			{
				var guid = character.Inventory.GetItemBySlot(i).RpgObject.Guid;
				var quantity = character.Inventory.GetQuantityFromSlot(i);
				saveData.ItemsAndQuantities.Add(new(guid,quantity));
			}

			// Shop inventory
			if (character is NPC npc)
			{
				if (npc.ShopInventory != null)
				{
					for (int i = 0; i < npc.ShopInventory.ItemCount; i++)
					{
						var guid = npc.ShopInventory.GetItemBySlot(i).RpgObject.Guid;
						var quantity = npc.ShopInventory.GetQuantityFromSlot(i);
						saveData.ItemsAndQuantities.Add(new(guid,quantity));
					}
				}
			}
			else if (character is PlayerCharacter player)
			{
				saveData.QuestProgress = player.QuestProgress;
			}

			return JsonConvert.SerializeObject(saveData, WolfRPG.Core.Settings.JsonSerializerSettings);
		}
	}
}