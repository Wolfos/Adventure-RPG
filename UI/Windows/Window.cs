using UnityEngine;

namespace UI
{
	public abstract class Window: MonoBehaviour
	{
		[SerializeField] private Canvas canvas;
		public bool pauseWhenOpen = true;
		public bool closeAble = true;
		public bool disablePlayerControls = false;

		public bool Active => gameObject.activeSelf;

		public void Open()
		{
			gameObject.SetActive(true);
		}

		public void Close()
		{
			gameObject.SetActive(false);
		}

		public void SetSortingOrder(int sortingOrder)
		{
			canvas.sortingOrder = sortingOrder;
		}

		public int GetSortingOrder()
		{
			return canvas.sortingOrder;
		}
	}
}