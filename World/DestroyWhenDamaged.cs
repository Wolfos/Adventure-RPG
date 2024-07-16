using Character;
using Interface;
using UnityEngine;

namespace World
{
    public class DestroyWhenDamaged : MonoBehaviour, IDamageTaker
    {
        [SerializeField] private GameObject destructionParticles;
        public void TakeDamage(float damage, float knockback, Vector3 point, CharacterBase other, Vector3 hitDirection = new Vector3())
        {
            if (destructionParticles != null)
            {
                var transform1 = transform;
                Instantiate(destructionParticles, transform1.position, transform1.rotation);
            }
            
            Destroy(gameObject);
        }

        public bool CanTakeDamage()
        {
            return true;
        }
    }
}