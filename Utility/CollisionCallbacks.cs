using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Utility
{
	public class CollisionCallbacks : MonoBehaviour
	{
		public Action<Collider> onTriggerEnter;
		private void OnTriggerEnter(Collider other)
		{
			onTriggerEnter?.Invoke(other);
		}

		public Action<Collider> onTriggerStay;
		private void OnTriggerStay(Collider other)
		{
			onTriggerStay?.Invoke(other);
		}
		
		public Action<Collider> onTriggerExit;
		private void OnTriggerExit(Collider other)
		{
			onTriggerExit?.Invoke(other);
		}
	}
}