using System;
using Data;
using Player;
using UnityEngine;
using UnityEngine.UI;
using WolfRPG.Core.Localization;
using WolfRPG.Core.Statistics;

namespace UI
{
    public class StatDisplay : MonoBehaviour
    {
        [SerializeField] private bool displayLevel;
        public Skill skill;
        [SerializeField] private Text statField;
        [SerializeField] private Text skillNameField;
        [SerializeField] private Slider slider;

        public void Initialize()
        {
            var playerData = PlayerCharacter.GetData();
            if (displayLevel)
            {
                skillNameField.text = LocalizationFile.Get("Level");
                statField.text = playerData.GetLevel().ToString();
                var levelProgress = playerData.GetProgressTowardsNextLevel();
                slider.value = LevelingSystemConfiguration.GetNormalizedLevelProgress(levelProgress);
            }
            else
            {
                skillNameField.text = LocalizationFile.Get($"Skill{skill}");
                var level = playerData.GetSkillValue(skill);
                statField.text = level.ToString();
                var xp = playerData.GetSkillXP(skill);
                slider.value = LevelingSystemConfiguration.GetNormalizedSkillLevelProgress(xp, level);
            }
        }
    }
}