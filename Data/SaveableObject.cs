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
        [HideInInspector] public bool global;
        private static SaveGameManager SaveGameManager => SystemContainer.GetSystem<SaveGameManager>();
        
        protected virtual void Start()
        {
            SaveGameManager.Register(this);

            if (!SaveGameManager.IsLoading)
            {
                var data = SaveGameManager.GetData(this);
                if (data != string.Empty)
                {
                    Load(data);
                }
            }
        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying == false)
            {
                return;
            }
            
            SaveGameManager.Unregister(this);
        }

        public abstract string Save();
        public abstract void Load(string json);
    }
}