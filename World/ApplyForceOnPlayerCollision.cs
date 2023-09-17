using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class ApplyForceOnPlayerCollision : MonoBehaviour
    {
        [SerializeField] private float force = 10;
        private Rigidbody _rigidbody;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("LocalPlayer") == false) return;
            
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            var relativePosition = transform.position - other.transform.position;
            _rigidbody.AddForce(relativePosition * force);
        }
    }
} 