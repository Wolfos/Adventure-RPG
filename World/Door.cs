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
        private class DoorData : ISaveData
        {
            public bool IsOpen { get; set; }    
        }

        public bool isProxy;
        [SerializeField] private float openRotation = 130;
        [SerializeField] private float closedRotation = 0;
        [SerializeField] private float openTime = 0.5f;
        [SerializeField] private float closeTime = 0.5f;
        [SerializeField] private Vector3 openOffset;

        private DoorData _data;
        private bool _changingState;
        private Collider _collider;
        private NavMeshObstacle _navMeshObstacle;
        private Vector3 _startPosition;
        
        private void Awake()
        {
            _collider = GetComponent<Collider>();
            _navMeshObstacle = GetComponent<NavMeshObstacle>();
            _startPosition = transform.position;
        }

        private void Start()
        {
            if (SaveGameManager.HasData(id))
            {
                _data = SaveGameManager.GetData(id) as DoorData;
                ProcessStateInstant();
            }
            else
            {
                _data = new();
                SaveGameManager.Register(id, _data);
            }
        }
        

        public void OnCanInteract(CharacterBase characterBase)
        {
            // TODO: Localize
            Tooltip.Activate(_data.IsOpen ? "Close" : "Open");
        }

        public void OnInteract(CharacterBase characterBase)
        {
            if (_changingState) return;
            
            if (_data.IsOpen)
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
            var transform1 = transform;
            if (_data.IsOpen)
            {
                transform1.localRotation = Quaternion.Euler(0, openRotation, 0);
                transform1.position = _startPosition + openOffset;
            }
            else
            {
                transform1.localRotation = Quaternion.Euler(0, closedRotation, 0);
                transform1.position = _startPosition;
            }
            if (_navMeshObstacle != null)
            {
                _navMeshObstacle.enabled = _data.IsOpen == false;
            }
        }

        private IEnumerator Open()
        {
            if (_collider != null) _collider.enabled = false;
            
            _changingState = true;
            var startRotation = transform.localRotation.eulerAngles.y;
            var endPosition = _startPosition + openOffset;
            for (float t = 0; t < openTime; t += Time.deltaTime)
            {
                var newRotation = Mathf.SmoothStep(startRotation, openRotation, t / openTime);
                var newPosition = Vector3.Lerp(_startPosition, endPosition, t / openTime);
                transform.localRotation = Quaternion.Euler(0, newRotation, 0);
                transform.position = newPosition;
                yield return null;
            }

            _changingState = false;
            _data.IsOpen = true;
            ProcessStateInstant();
            
            if (_collider != null) _collider.enabled = true;
        }
        
        private IEnumerator Close()
        {
            if (_collider != null) _collider.enabled = false;
            
            _changingState = true;
            var startRotation = transform.localRotation.eulerAngles.y;
            var startPosition = transform.position;
            var endPosition = _startPosition;
            for (float t = 0; t < closeTime; t += Time.deltaTime)
            {
                var newRotation = Mathf.SmoothStep(startRotation, closedRotation, t / closeTime);
                var newPosition = Vector3.Lerp(startPosition, endPosition, t / closeTime);
                transform.localRotation = Quaternion.Euler(0, newRotation, 0);
                transform.position = newPosition;
                yield return null;
            }

            _changingState = false;
            _data.IsOpen = false;
            ProcessStateInstant();
            
            if (_collider != null) _collider.enabled = true;
        }
    }
}