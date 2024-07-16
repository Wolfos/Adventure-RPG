using UnityEngine;
using WolfRPG.Core.Statistics;

namespace Items
{
	[CreateAssetMenu(fileName = "New Weapon", menuName = "eeStudio/Weapon Data")]
	public class MeleeWeaponData: EquipmentData
	{
		public float BaseDamage;
		public float Knockback;
		public float AttackDuration;
		public AudioClip AttackSound;
		public AudioClip HitSound;
		public AudioClip BlockSound;

		public GameObject HitParticles;

		public Skill AssociatedSkill;
	}
}