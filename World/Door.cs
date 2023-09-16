using System;
using System.Collections;
using Character;
using Data;
using Interface;
using UI;
using UnityEngine;
using UnityEngine.AI;

namespace World
{
    public class Door : SaveableObject, IInteractable
    {
        [SerializeField] private float openRotation = 130;
        [SerializeField] private float closedRotation = 0;
        [SerializeField] private float openTime = 0.5f;
        [SerializeField] private float closeTime = 0.5f;
        
        private bool _isOpen;
        private bool _changingState;
        private Collider _collider;
        private NavMeshObstacle _navMeshObstacle;

        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _navMeshObstacle = GetComponent<NavMeshObstacle>();
        }

        public override string Save()
        {
            return _isOpen ? "open" : "closed";
        }

        public override void Load(string json)
        {
            if (json == "open")
            {
                _isOpen = true;
            }
            else
            {
                _isOpen = false;
            }
            ProcessStateInstant();
        }

        public void OnCanInteract(CharacterBase characterBase)
        {
            // TODO: Localize
            Tooltip.Activate(_isOpen ? "Close" : "Open", transform, new (1, 1));
        }

        public void OnInteract(CharacterBase characterBase)
        {
            if (_changingState) return;
            
            if (_isOpen)
            {
                StartCoroutine(Close());
            }
            else
            {
                StartCoroutine(Open());
            }
            
            characterBase.EndInteraction(_collider);
        }

        public void OnEndInteract(CharacterBase characterBase)
        {
            Tooltip.DeActivate();
        }

        private void ProcessStateInstant()
        {
            transform.localRotation = Quaternion.Euler(0, _isOpen ? openRotation : closedRotation, 0);
            if (_navMeshObstacle != null)
            {
                _navMeshObstacle.enabled = _isOpen == false;
            }
        }

        private IEnumerator Open()
        {
            if (_collider != null) _collider.enabled = false;
            
            _changingState = true;
            var startRotation = transform.localRotation.eulerAngles.y;
            for (float t = 0; t < openTime; t += Time.deltaTime)
            {
                var newRotation = Mathf.SmoothStep(startRotation, openRotation, t / openTime);
                transform.localRotation = Quaternion.Euler(0, newRotation, 0);
                yield return null;
            }

            _changingState = false;
            _isOpen = true;
            ProcessStateInstant();
            
            if (_collider != null) _collider.enabled = true;
        }
        
        private IEnumerator Close()
        {
            if (_collider != null) _collider.enabled = false;
            
            _changingState = true;
            var startRotation = transform.localRotation.eulerAngles.y;
            for (float t = 0; t < closeTime; t += Time.deltaTime)
            {
                var newRotation = Mathf.SmoothStep(startRotation, closedRotation, t / openTime);
                transform.localRotation = Quaternion.Euler(0, newRotation, 0);
                yield return null;
            }

            _changingState = false;
            _isOpen = false;
            ProcessStateInstant();
            
            if (_collider != null) _collider.enabled = true;
        }
    }
}