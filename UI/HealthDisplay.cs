using System.Collections.Generic;
using UnityEngine;

namespace UI
{
	public class HealthDisplay : MonoBehaviour
	{
		[SerializeField]
		private GameObject heartPrefab;

		private List<Heart> hearts;
		
		private float _currentHealth;

		public float CurrentHealth
		{
			get { return _currentHealth; }
			set
			{
				_currentHealth = value;
				for (int i = 0; i < hearts.Count; i++)
				{
					hearts[i].Health = Mathf.Clamp01(_currentHealth - i);
				}
			}
		}

		private float _maxHealth;

		public float MaxHealth
		{
			get { return _maxHealth; }
			set
			{
				_maxHealth = value;

				if(hearts == null) hearts = new List<Heart>();
				else
				{
					foreach (var heart in hearts)
					{
						Destroy(heart.gameObject);
					}
					hearts.Clear();
				}
				
				for (int i = 0; i < _maxHealth; i++)
				{
					var heart = Instantiate(heartPrefab, transform);
					Heart h = heart.GetComponent<Heart>();
					hearts.Add(h);
				}
			}
		}
		
	
	}
}