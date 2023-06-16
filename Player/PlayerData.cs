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
