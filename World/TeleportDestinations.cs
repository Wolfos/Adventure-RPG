using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    public class TeleportDestinations : MonoBehaviour
    {
        [Serializable]
        public struct Destination
        {
            public string Name;
            public Transform Transform;
            public WorldSpace WorldSpace;
            public bool IsFastTravelLocation;
            public Sprite MapIcon;
        }
        
        private static TeleportDestinations _instance;
        
        [SerializeField] private Destination[] destinations;
        
        private void Awake()
        {
            _instance = this;
        }

        public static IEnumerable<Destination> GetAllUnlockedFastTravelLocations()
        {
            // TODO: Unlocking
            return _instance.destinations.Where(d => d.IsFastTravelLocation);
        }

        public static Destination GetDestination(int i)
        {
            if (i > _instance.destinations.Length)
            {
                i = 0;
                Debug.LogError($"Destination {i} doesn't exist");
            }
            
            return _instance.destinations[i];
        }

        public static Destination GetDestination(string name)
        {
            return _instance.destinations.FirstOrDefault(d => string.Equals(d.Name, name, StringComparison.CurrentCultureIgnoreCase));
        }
    }
}