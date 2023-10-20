using System;
using Sirenix.OdinInspector;
using UnityEngine;
using Utility;


namespace Data
{
    public abstract class SaveableObject : MonoBehaviour
    {
        [Button("Generate ID")]
        private void GenerateID()
        {
            id = Guid.NewGuid().ToString();
        }
        
        public string id;
        protected ISaveData SaveData { get; set; }
    }
}