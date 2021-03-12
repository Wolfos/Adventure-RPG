using System;
using System.Collections.Generic;
using Character;

namespace Data
{
	public class CharacterPool
	{
		private static Dictionary<Guid, CharacterBase> characters = new Dictionary<Guid, CharacterBase>();

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
			characters.Add(id, character);
			return id;
		}

		public static void Unregister(string id)
		{
			Unregister(Guid.Parse(id));
		}

		public static void Unregister(Guid id)
		{
			characters.Remove(id);
		}
		
		public static CharacterBase GetCharacter(string id)
		{
			return string.IsNullOrEmpty(id) ? null : GetCharacter(Guid.Parse(id));
		}

		public static CharacterBase GetCharacter(Guid id)
		{
			if (characters.ContainsKey(id))
			{
				return characters[id];
			}

			return null;
		}

		public static void Clear()
		{
			characters.Clear();
		}
	}
}