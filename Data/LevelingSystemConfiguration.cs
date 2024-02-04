using System;
using System.Collections.Generic;
using WolfRPG.Core;
using WolfRPG.Core.Statistics;

namespace Data
{
	public class LevelingSystemConfiguration: IRPGComponent
	{
		public class SkillIntTuple
		{
			public Skill Skill { get; set; }
			public int Int { get; set; }
		}
		public int BaseXpPerLevel { get; set; } = 300;
		public float LevelingGainMultiplier { get; set; } = 0.007f;
		public SkillIntTuple[] XpGainPerAction { get; set; }
		public int SkillLevelsRequiredForLevel { get; set; } = 10;

		private readonly Dictionary<Skill, int> _xpGainPerAction = new();

		private static LevelingSystemConfiguration _instance;

		public LevelingSystemConfiguration()
		{
			_instance = this;
		}

		public int GetXpGainPerAction(Skill skill)
		{
			if (_xpGainPerAction.Count == 0) // Fill dictionary for faster lookups on later calls
			{
				foreach (var t in XpGainPerAction)
				{
					_xpGainPerAction.Add(t.Skill, t.Int);
				}
			}
			
			return _xpGainPerAction[skill];
		}

		public static int GetXpRequiredForLevel(int level)
		{
			return (int) (_instance.BaseXpPerLevel + _instance.BaseXpPerLevel * ((float)(level * level) * _instance.LevelingGainMultiplier));
		}

		public static float GetNormalizedLevelProgress(float progress)
		{
			return (float) progress / (float)_instance.SkillLevelsRequiredForLevel;
		}

		public static float GetNormalizedSkillLevelProgress(float xp, int level)
		{
			var requiredXp = GetXpRequiredForLevel(level);
			return (float) xp / (float) requiredXp;
		}
	}
}