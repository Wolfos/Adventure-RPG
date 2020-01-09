using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Heart : MonoBehaviour
	{
		[SerializeField] private Image[] heartPieces;
		private float _health;
		public float Health
		{
			get { return _health; }
			set
			{
				_health = value;
				for (int i = 0; i < heartPieces.Length; i++)
				{
					heartPieces[i].enabled = (_health > ((float) i) / 4);
				}
			}
		}
		
	}
}