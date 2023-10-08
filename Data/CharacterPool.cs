using System;
using System.Collections.Generic;
using Character;

namespace Data
{
	public class CharacterPool
	{
		private static Dictionary<Guid, CharacterBase> _characters = new();
		private static CharacterBase _player; // Placeholder until we get a proper faction system

		public static Guid Register(CharacterBase character)
		{
			var id = Guid.NewGuid();
			return Register(id, character);
		}

		public static Guid Register(string id, CharacterBase character)
		{
			return Register(Guid.Parse(id), character);
		}

		public static Guid Register(Guid id, CharacterBase character)
		{
			_characters.Add(id, character);
			return id;
		}

		public static void Unregister(string id)
		{
			Unregister(Guid.Parse(id));
		}

		public static void Unregister(Guid id)
		{
			_characters.Remove(id);
		}
		
		public static CharacterBase GetCharacter(string id)
		{
			return string.IsNullOrEmpty(id) ? null : GetCharacter(Guid.Parse(id));
		}

		public static void RegisterPlayer(CharacterBase player)
		{
			_player = player;
		}
		
		public static CharacterBase GetPlayer()
		{
			return _player;
		}

		public static CharacterBase GetCharacter(Guid id)
		{
			if (_characters.ContainsKey(id))
			{
				return _characters[id];
			}

			return null;
		}

		public static void Clear()
		{
			_characters.Clear();
		}
	}
}