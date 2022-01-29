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
        
        // Attack animation hit moment
        [Preserve]
        private void Hit()
        {
            onHit?.Invoke();    
        }
    }
}