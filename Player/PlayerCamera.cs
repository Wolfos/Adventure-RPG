using UnityEngine;
using UnityEngine.InputSystem;
using Utility;

namespace Player
{
	public class PlayerCamera: MonoBehaviour
	{
		[SerializeField] private float followSpeed;
		[SerializeField] private float rotationSpeed;
		[SerializeField] private Transform verticalPivot;
		[SerializeField] private Range verticalClamp;
		[SerializeField] private Transform camera;
		[SerializeField] private Range cameraDistance;
		[SerializeField] private float zoomSpeed;
		[SerializeField] private Transform targetTransform;
		[SerializeField] private Vector3 offset;
		[SerializeField] private LayerMask blockLayerMask;
		[SerializeField] private float recoverySmoothness = 0.5f;
		[SerializeField] private float zoomSmoothness = 0.35f;
		
		private Vector2 _movementInput;
		private float _zoomInput;
		private float _desiredZoom;
		private bool _wasBlocked;

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
			_desiredZoom = camera.localPosition.z;
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
			var localPosition = camera.localPosition;
			var position = localPosition;
			
			if (_zoomInput != 0 && _wasBlocked)
			{
				_wasBlocked = false; // Zoom input cancels slow recovery
				_desiredZoom = position.z;
			}
			
			_desiredZoom += _zoomInput * zoomSpeed;
			_desiredZoom = Mathf.Clamp(_desiredZoom, cameraDistance.start, cameraDistance.end);
		
			if (_wasBlocked) // Slow recovery after being blocked
			{
				position.z = Mathf.Lerp(position.z, _desiredZoom, recoverySmoothness);
			}
			else
			{
				position.z = Mathf.Lerp(position.z, _desiredZoom, zoomSmoothness); // Smooth towards desired zoom position
			}
			
			
			// Check if visibility is blocked and zoom in if true
			Physics.queriesHitBackfaces = true;
			if (Physics.Raycast(transform.position, -camera.forward, out var hit, Mathf.Abs(position.z), blockLayerMask))
			{
				if (hit.collider.isTrigger == false)
				{
					var distance = Vector3.Distance(hit.point, transform.position);
					position.z = Mathf.Lerp(localPosition.z, -distance, 0.5f);
					
					_wasBlocked = true;
				}
			}

			Physics.queriesHitBackfaces = false;
			
			position.z = Mathf.Clamp(position.z, cameraDistance.start, cameraDistance.end);
			
			camera.localPosition = position;
		}
		
		private void LateUpdate()
		{
			var targetPosition = targetTransform.position + offset;
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