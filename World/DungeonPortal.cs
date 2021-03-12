using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class DungeonPortal : MonoBehaviour
    {
        [SerializeField] private DungeonPortal partner;
        [HideInInspector] public bool disabled;
        private static DateTime lastTeleport;
        
        private void OnTriggerEnter(Collider other)
        {
            if (lastTeleport > DateTime.Now.AddSeconds(-1)) return;
            if (disabled)
            {
                disabled = false;
                return;
            }
            if (other.CompareTag("LocalPlayer"))
            {
                other.gameObject.SetActive(false);
                other.transform.position = partner.transform.position;
                other.gameObject.SetActive(true);
                partner.disabled = true;
                lastTeleport = DateTime.Now;
            }
        }
    }
}