using System;
using UnityEngine;
using UnityEngine.Scripting;

namespace Character
{
    public class CharacterAnimationEvents : MonoBehaviour
    {
        public Action onHit;
        public Action onCanDoSecondAttack;
        public Action OnEndDoSecondAttack;
        
        // Attack animation hit moment
        [Preserve]
        private void Hit()
        {
            onHit?.Invoke();    
        }
        
        [Preserve]
        private void CanDoSecondAttack()
        {
            onCanDoSecondAttack?.Invoke();    
        }
        
        [Preserve]
        private void EndDoSecondAttack()
        {
            OnEndDoSecondAttack?.Invoke();    
        }

        [Preserve]
        private void FootL()
        {
        }
        
        [Preserve]
        private void FootR()
        {
        }
        
        [Preserve]
        private void Land()
        {
        }
    }
}