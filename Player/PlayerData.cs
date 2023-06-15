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

	public override void Load(string json)
	{
		playerCharacter.characterController.enabled = false;
		CharacterSaveUtility.Load(json, playerCharacter);
		playerCharacter.characterController.enabled = true;
		playerCharacter.RegisterCallbacks();
	}
		
	public override string Save()
	{
		return CharacterSaveUtility.GetSaveData(playerCharacter);
	}
}
