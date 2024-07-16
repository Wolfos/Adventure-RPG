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
        public Action OnFootL;
        public Action OnFootR;
        
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
            OnFootL?.Invoke();
        }
        
        [Preserve]
        private void FootR()
        {
            OnFootR?.Invoke();
        }
        
        [Preserve]
        private void Land()
        {
        }
        
    }
}