using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DraggableItem : MonoBehaviour
	{
		[HideInInspector]
		public Inventory inventory;
		[HideInInspector]
		public int slot;

		private Transform startParent;
		private Vector3 startPosition;
		private Vector2 startDifference;

		private bool dragging = false;

		private RectTransform rectTransform;

		private Button button;

		void Start()
		{
			rectTransform = GetComponent<RectTransform>();
			button = transform.parent.GetComponent<Button>();
		}

		public void Click()
		{
			if (!dragging) button.OnSubmit(null);
		}

		public void BeginDrag()
		{
			if (inventory.container.GetItemBySlot(slot) == null) return;

			startParent = transform.parent;
			startPosition = transform.position;
			startDifference = Input.mousePosition - startPosition;
			transform.SetParent(transform.parent.parent.parent);

			dragging = true;
		}

		public void EndDrag()
		{
			if (dragging)
			{
				transform.SetParent(startParent);
				transform.position = startPosition;

				inventory.ItemDropped(slot);

				dragging = false;
			}
		}

		public void Drag()
		{
			Vector3 position = new Vector3();
			position.x = Input.mousePosition.x - startDifference.x;
			position.y = Input.mousePosition.y - startDifference.y;
			if (dragging)
			{
				transform.position = position;
				transform.Translate(Vector3.forward);
			}
		}
	}
}