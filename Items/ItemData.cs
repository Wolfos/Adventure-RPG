using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using WolfRPG.Core.Localization;

namespace Items
{
    [CreateAssetMenu(fileName = "New Item", menuName = "eeStudio/Basic Item", order = 1)]
    public class ItemData : ScriptableObject
    {
        [FormerlySerializedAs("_guid")]
        [Header("Base Item Data")]
        [SerializeField] private string guid;

        [Button("New GUID")]
        private void NewGuid()
        {
            Guid = Guid.NewGuid();
        }
        
        private Guid _cachedGuid;
        public Guid Guid {
            get
            {
                if (_cachedGuid == Guid.Empty)
                {
                    _cachedGuid = Guid.Parse(guid);
                }

                return _cachedGuid;
            }
            set
            {
                _cachedGuid = value;
                guid = value.ToString();
            }
        }
        
        // Visual representation of the item in the game world
        public GameObject prefab;

        public Sprite sprite;

        public LocalizedString Name;
        public LocalizedString Description;
        public ItemType Type;
        public int BaseValue;
        public float Weight;
    }
}