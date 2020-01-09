using UnityEngine;

namespace UI
{
	public class UIBase : MonoBehaviour
	{
		private static UIBase instance;

		[SerializeField] private GameObject healthDisplayPrefab;

		private void Awake()
		{
			instance = this;
		}
		
		public static HealthDisplay GetHealthDisplay()
		{
			var go = Instantiate(instance.healthDisplayPrefab, instance.transform);
			return go.GetComponent<HealthDisplay>();
		}
	}
}