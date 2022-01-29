using System;
using Data;
using UnityEngine;
using Utility;

namespace World
{
    [RequireComponent(typeof(Character.NPC))]
    public class StartQuestWhenKilled : MonoBehaviour
    {
        [SerializeField] private Quest quest;

        private void Start()
        {
            GetComponent<Character.NPC>().OnDeath += OnDeath;
        }

        private void OnDeath()
        {
            SystemContainer.GetSystem<Player.PlayerCharacter>().StartQuest(quest);
        }
    }
}