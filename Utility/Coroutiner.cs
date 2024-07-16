using UnityEngine;

namespace Utility
{
	public class Coroutiner: MonoBehaviour
	{
		private static Coroutiner _instance;
		public static Coroutiner Instance
		{
			get
			{
				if (_instance == null)
				{
					var go = new GameObject("Coroutine runner");
					_instance = go.AddComponent<Coroutiner>();
				}

				return _instance;
			}
		}
	}
}