using UnityEngine;

namespace World
{
    [CreateAssetMenu(menuName = "eeStudio/World Settings")]
    public class WorldSettings : ScriptableObject
    {
        private static WorldSettings _instance;
        private static WorldSettings Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Resources.Load<WorldSettings>("WorldSettings");
                }

                return _instance;
            }
        }
    }
}