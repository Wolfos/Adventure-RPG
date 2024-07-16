using System;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    public class MovingPlatform : MonoBehaviour
    {
        private List<CharacterController> _characterControllers = new();
        private Vector3 _lastPosition;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("MovingPlatformTrigger")) // TODO: Once NPC's use CharacterController, add those here too
            {
                var controller = other.gameObject.GetComponentInParent<CharacterController>();
                if (_characterControllers.Contains(controller) == false)
                {
                    Debug.Log("Add");
                    _characterControllers.Add(controller);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("MovingPlatformTrigger")) // TODO: Once NPC's use CharacterController, add those here too
            {
                var controller = other.gameObject.GetComponentInParent<CharacterController>();

                Debug.Log("Remove");
                _characterControllers.Remove(controller);
                
            }
        }

        private void LateUpdate()
        {
            var position = transform.position;
            var translation = position - _lastPosition;

            foreach (var controller in _characterControllers)
            {
                controller.transform.Translate(translation, Space.World);
            }
            
            _lastPosition = position;
        }
    }
}