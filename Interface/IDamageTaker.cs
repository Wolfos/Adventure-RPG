using Character;
using UnityEngine;

namespace Interface
{
	public interface IDamageTaker
	{
		public void TakeDamage(float damage, float knockback, Vector3 point, CharacterBase other, Vector3 hitDirection = new());

		public bool CanTakeDamage();
	}
}