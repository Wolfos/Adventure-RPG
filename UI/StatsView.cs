using System;
using System.Collections.Generic;
using UnityEngine;
using WolfRPG.Core.Statistics;

namespace UI
{
    public class StatsView : MonoBehaviour
    {
        private List<StatDisplay> _statDisplays = new();
        [SerializeField] private StatDisplay statDisplayPrefab;
        [SerializeField] private StatDisplay levelDisplay;
        private void OnEnable()
        {
            levelDisplay.Initialize();
            // Update
            if (_statDisplays.Count > 0)
            {
                foreach (var sd in _statDisplays)
                {
                    sd.Initialize();
                }

                return;
            }
            
            // Create anew
            for (int i = 1; i < (int) Skill.MAX; i++)
            {
                var statDisplay = Instantiate(statDisplayPrefab, statDisplayPrefab.transform.parent);
                _statDisplays.Add(statDisplay);

                statDisplay.skill = (Skill) i;
                statDisplay.Initialize();
            }
            
            statDisplayPrefab.gameObject.SetActive(false);
        }
    }
}