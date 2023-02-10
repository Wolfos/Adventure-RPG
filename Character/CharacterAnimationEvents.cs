using System;
using System.Collections;
using System.Collections.Generic;
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
    }
}