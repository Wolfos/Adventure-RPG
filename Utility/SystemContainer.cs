/*
	SystemContainer.cs
	Created 4/15/2017 2:50:45 PM
	Project RPG by Robin van Ee
*/
using UnityEngine;
using System.Collections.Generic;

namespace Utility
{
	public class SystemContainer : MonoBehaviour 
	{
        private static SystemContainer instance;
        private Dictionary<System.Type, object> systems;

        void Awake()
        {
            instance = this;
            systems = new Dictionary<System.Type, object>();
        }

        public static void Register(object obj)
        {
            instance.systems.Add(obj.GetType(), obj);
        }

        public static T GetSystem<T>()
        {
            object system;
            instance.systems.TryGetValue(typeof(T), out system);
            if (system == null) Debug.LogError("System of type " + typeof(T) + " was not found");
            return (T)system;
        }
    }
}