using System;
using System.Collections.Generic;
using Data;
using Newtonsoft.Json;
using WolfRPG.Character;
using Attribute = WolfRPG.Core.Statistics.Attribute;

namespace Character
{
	public static class CharacterSaveUtility
	{
		private class CharacterSaveData
		{
			public CharacterData CharacterData { get; set; }
			// GUID, item quantity
			public List<Tuple<string, int>> ItemsAndQuantities { get; set; }
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

			if (saveData.ItemsAndQuantities != null)
			{
				foreach (var tuple in saveData.ItemsAndQuantities)
				{
					character.Inventory.AddItem(tuple.Item1, tuple.Item2);
				}
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
				ItemsAndQuantities = new()
			};

			for (int i = 0; i < character.Inventory.ItemCount; i++)
			{
				var guid = character.Inventory.GetItemBySlot(i).RpgObject.Guid;
				var quantity = character.Inventory.GetQuantityFromSlot(i);
				saveData.ItemsAndQuantities.Add(new(guid,quantity));
			}

			return JsonConvert.SerializeObject(saveData, WolfRPG.Core.Settings.JsonSerializerSettings);
		}
	}
}