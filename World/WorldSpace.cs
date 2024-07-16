using System;

namespace World
{
	public enum WorldSpace
	{
		World, CastleDungeon, DwarvenDungeonCity
	}

	public static class WorldSpaces
	{
		public static string GetSceneName(WorldSpace worldSpace)
		{
			switch (worldSpace)
			{
				case WorldSpace.World:
					return "Terrains";
				case WorldSpace.CastleDungeon:
					return "Castle";
				case WorldSpace.DwarvenDungeonCity:
					return "DwarvenDungeonCity";
				default:
					throw new ArgumentOutOfRangeException(nameof(worldSpace), worldSpace, null);
			}
		}
	}
}