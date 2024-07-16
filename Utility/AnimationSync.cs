using Player;
using UnityEngine;

namespace Utility
{
    public class AnimationSync : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        
        private float _lastUpdateTime;
        private float _updateDelta;

        private const float CullingDistanceSquared = 100.0f;
        
        private void FixedUpdate()
        {
            if (Vector3.SqrMagnitude(transform.position - PlayerCamera.GetCameraPosition()) > CullingDistanceSquared)
            {
                return;
            }
            
            var delta = Time.time - _lastUpdateTime;
            animator.Update(delta);
            
            _updateDelta += delta;
            _lastUpdateTime = Time.time;
        }

        private void Update()
        {
            if (Vector3.SqrMagnitude(transform.position - PlayerCamera.GetCameraPosition()) > CullingDistanceSquared)
            {
                return;
            }
            
            animator.Update(-_updateDelta);

            _updateDelta = 0;
            _lastUpdateTime = Time.time;
        }
    }
}