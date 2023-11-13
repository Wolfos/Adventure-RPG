using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
    public class AnimationSync : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private float _lastUpdateTime;
        private float _updateDelta;
        
        private void FixedUpdate()
        {
            var delta = Time.time - _lastUpdateTime;
            animator.Update(delta);
            
            _updateDelta += delta;
            _lastUpdateTime = Time.time;
        }

        private void Update()
        {
            animator.Update(-_updateDelta);

            _updateDelta = 0;
            _lastUpdateTime = Time.time;
        }
    }
}