using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;


namespace Data
{
    public abstract class SaveableObject : MonoBehaviour
    {
        public string id;
        
        protected virtual void Start()
        {
            SystemContainer.GetSystem<SaveGameManager>().Register(this);
        }
        
        public abstract string Save();
        public abstract void Load(string json);
    }
}