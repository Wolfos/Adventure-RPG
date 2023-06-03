using Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI
{
	public class DraggableItem : MonoBehaviour
	{
		public InventoryView InventoryView { get; set; }
		public int Slot { get; set; }
		public bool IsDragable { get; set; }
		
		[SerializeField] private Canvas canvas;
		
		private Vector2 _startPosition;
		private Vector2 _startDifference;
		private bool _dragging;
		private Button _button;

		void Start()
		{
			_button = transform.parent.GetComponent<Button>();
		}

		public void Click()
		{
			if (!_dragging) _button.OnSubmit(null);
		}

		public void BeginDrag()
		{
			if (!IsDragable) return;
			if (InventoryView.Container.GetItemBySlot(Slot) == null) return;

			_startPosition = transform.position;
			_startDifference = InputMapper.MousePosition - _startPosition;
			
			canvas.overrideSorting = true;
			canvas.sortingOrder = 10;

			_dragging = true;
		}

		public void EndDrag()
		{
			if (!IsDragable) return;
			if (_dragging)
			{
				transform.position = _startPosition;

				InventoryView.ItemDropped(Slot);

				canvas.overrideSorting = false;

				_dragging = false;
			}
		}

		public void Drag()
		{
			if (!IsDragable) return;
			Vector3 position = new Vector3();
			position.x = InputMapper.MousePosition.x - _startDifference.x;
			position.y = InputMapper.MousePosition.y - _startDifference.y;
			if (_dragging)
			{
				transform.position = position;
				transform.Translate(Vector3.forward);
			}
		}
	}
}