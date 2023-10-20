using System;
using Data;
using Character;
using Player;
using UnityEngine;
using UnityEngine.Serialization;

public class PlayerData : SaveableObject
{
	[FormerlySerializedAs("player")] [SerializeField] private PlayerCharacter playerCharacter;

	private class PlayerSaveData : ISaveData
	{
		public string Json { get; set; }
	}

	private PlayerSaveData _saveData;

	private void Start()
	{
		_saveData = new();
		
		if (SaveGameManager.HasData(id))
		{
			playerCharacter.characterController.enabled = false;
			
			_saveData = SaveGameManager.GetData(id) as PlayerSaveData;
			CharacterSaveUtility.Load(_saveData.Json, playerCharacter);
			
			playerCharacter.characterController.enabled = true;
			playerCharacter.OnFinishedLoading();
		}
		else
		{
			SaveGameManager.Register(id, _saveData);
		}

		SaveGameManager.OnSave += UpdateSaveData;
	}

	private void OnDestroy()
	{
		UpdateSaveData();
		SaveGameManager.OnSave -= UpdateSaveData;
	}

	private void UpdateSaveData()
	{
		_saveData.Json = CharacterSaveUtility.GetSaveData(playerCharacter);
	}
}
