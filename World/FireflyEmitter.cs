using System;
using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

namespace World
{
    public class FireflyEmitter : MonoBehaviour
    {
        [SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
        [SerializeField] private TimeStamp offTime = new TimeStamp(7,0);
        [SerializeField] private new ParticleSystem particleSystem;
        [SerializeField] private float spawnDistance = 0.2f;
        
        private List<Transform> _transforms = new();
        private List<Vector3> _lastPositions = new();
        private void OnTriggerEnter(Collider other)
        {
            if (TimeManager.IsBetween(onTime, offTime) == false) return;
            
            if (other.gameObject.CompareTag("LocalPlayer")) // TODO: NPC tag
            {
                if (_transforms.Contains(other.transform) == false)
                {
                    _transforms.Add(other.transform);
                    _lastPositions.Add(other.transform.position);
                }
            }
        }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("LocalPlayer")) // TODO: NPC tag
            {
                if (_transforms.Contains(other.transform))
                {
                    var index = _transforms.IndexOf(other.transform);
                    _transforms.RemoveAt(index);
                    _lastPositions.RemoveAt(index);
                }
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < _transforms.Count; i++)
            {
                var position = _transforms[i].position;
                var lastPosition = _lastPositions[i];
                if ((position - lastPosition).sqrMagnitude >= spawnDistance * spawnDistance)
                {
                    particleSystem.transform.position = position;
                    particleSystem.Play();
                    _lastPositions[i] = position;
                }
            }
        }
    }
}