using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using Utility;

namespace Player
{
	public class PlayerCamera : MonoBehaviour
	{
		[SerializeField] private float followSpeed;
		[SerializeField] private float rotationSpeed;
		[SerializeField] private Transform verticalPivot;
		[SerializeField] private Range verticalClamp;
		[SerializeField] private Transform camera;
		[SerializeField] private Range cameraDistance;
		[SerializeField] private float zoomSpeed;
		[SerializeField] private Transform targetTransform;
		
		private Vector3 _offset;
		private Vector2 _movementInput;
		private float _zoomInput;

		private void OnEnable()
		{
			EventManager.OnCameraMove += OnCameraMove;
			EventManager.OnCameraZoom += OnCameraZoom;
		}

		private void OnDisable()
		{
			EventManager.OnCameraMove -= OnCameraMove;
			EventManager.OnCameraZoom -= OnCameraZoom;
		}


		private void Start()
		{
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
			_offset = targetTransform.position - transform.position;
		}

		private void Update()
		{
			if (PlayerControls.InputActive == false) return;
			
			// Rotation
			transform.Rotate(Vector3.up, -_movementInput.x * rotationSpeed);
			verticalPivot.Rotate(Vector3.right, -_movementInput.y * rotationSpeed);

			var eulerAngles = verticalPivot.rotation.eulerAngles;
			if (eulerAngles.x > 180 && eulerAngles.x < verticalClamp.start)
			{
				eulerAngles.x = verticalClamp.start;
			}
			else if (eulerAngles.x < 180 && eulerAngles.x > verticalClamp.end)
			{
				eulerAngles.x = verticalClamp.end;
			}
			eulerAngles.y = 0;
			eulerAngles.z = 0;
			verticalPivot.localRotation = Quaternion.Euler(eulerAngles);
			
			// Zoom
			var position = camera.localPosition;
			position.z += _zoomInput * zoomSpeed;
			position.z = Mathf.Clamp(position.z, cameraDistance.start, cameraDistance.end);
			camera.localPosition = position;
		}
		
		private void LateUpdate()
		{
			var targetPosition = targetTransform.position - _offset;
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
		}
		
		#region Input

		public void OnCameraMove(InputAction.CallbackContext context)
		{
			_movementInput = context.ReadValue<Vector2>();
		}
		
		public void OnCameraZoom(InputAction.CallbackContext context)
		{
			_zoomInput = context.ReadValue<float>();
		}
		#endregion
	}
}