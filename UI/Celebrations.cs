using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Utility;
using WolfRPG.Core.Localization;
using WolfRPG.Core.Statistics;

namespace UI
{

    public class Celebrations : MonoBehaviour
    {
        private static Celebrations _instance;
        [SerializeField] private AudioClip minorLevelupSound;
        [SerializeField] private AudioClip majorLevelupSound;
        [SerializeField] private TextMeshProUGUI minorLevelupText;
        [SerializeField] private TextMeshProUGUI majorLevelupText;

        private void Awake()
        {
            _instance = this;
        }

        private IEnumerator MinorLevelUpCelebration(float delay)
        {
            yield return new WaitForSeconds(delay);
            SFXPlayer.PlaySound(minorLevelupSound);
            minorLevelupText.enabled = true;
            var color = minorLevelupText.color;
            color.a = 0;
            for (float t = 0; t < 1; t += Time.deltaTime * 2)
            {
                color.a = Mathf.SmoothStep(0, 1, t);
                minorLevelupText.color = color;
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);
            
            for (float t = 0; t < 1; t += Time.deltaTime * 2)
            {
                color.a = Mathf.SmoothStep(1, 0, t);
                minorLevelupText.color = color;
                yield return null;
            }

            minorLevelupText.enabled = false;
        }
        
        private IEnumerator MajorLevelupCelebration()
        {
            yield return new WaitForSeconds(4); // Wait until minor celebration is over
            SFXPlayer.PlaySound(majorLevelupSound);
            majorLevelupText.enabled = true;
            var color = majorLevelupText.color;
            color.a = 0;
            for (float t = 0; t < 1; t += Time.deltaTime * 2)
            {
                color.a = Mathf.SmoothStep(0, 1, t);
                majorLevelupText.color = color;
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);
            
            for (float t = 0; t < 1; t += Time.deltaTime * 2)
            {
                color.a = Mathf.SmoothStep(1, 0, t);
                majorLevelupText.color = color;
                yield return null;
            }

            majorLevelupText.enabled = false;
        }

        public static void MinorLevelUp(Skill skill, int newLevel)
        {
            float delay = 0;
            if (_instance.majorLevelupText.enabled || _instance.minorLevelupText.enabled) // If one is already playing, delay this
            {
                delay = 4f;
            }
            var text = _instance.minorLevelupText;
            text.text = LocalizationFile.Get($"Skill{skill}") + $"\n{newLevel}";
            _instance.StartCoroutine(_instance.MinorLevelUpCelebration(delay));
        }
        
        public static void MajorLevelUp(int newLevel)
        {
            var text = _instance.majorLevelupText;
            text.text = LocalizationFile.Get("Level") + $"\n{newLevel}";
            _instance.StartCoroutine(_instance.MajorLevelupCelebration());
        }
    }
}