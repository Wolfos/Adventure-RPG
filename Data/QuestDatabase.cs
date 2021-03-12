using UnityEngine;

namespace Data
{
    public class QuestDatabase : MonoBehaviour
    {
        public Quest[] quests;

        private void Awake()
        {
            for (int i = 0; i < quests.Length; i++)
            {
                quests[i].id = i;
            }
        }
    }
}