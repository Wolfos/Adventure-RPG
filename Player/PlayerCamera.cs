using System;
using System.Collections;
using Character;
using UnityEngine;
using UnityEngine.InputSystem;
using Utility;
using Random = UnityEngine.Random;

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
		[SerializeField] private Vector3 dialogueOffset;
		[SerializeField] private LayerMask blockLayerMask;
		[SerializeField] private float recoverySmoothness = 0.5f;
		[SerializeField] private float zoomSmoothness = 0.35f;
		[SerializeField] private float dialogueAngle = 20;
		[SerializeField] private float dialogueCameraSpeed = 2;
		[SerializeField] private float dialogueZoom = 2;
		
		private Vector2 _movementInput;
		private float _zoomInput;
		private float _desiredZoom;
		private bool _wasBlocked;

		private Vector3 _dialogueStartPosition;
		private Quaternion _dialogueStartRotation;
		private Coroutine _smoothRoutine;
		private bool _movementOverridenByRoutine;
		private Vector3 _dialogueStartCameraPosition;
		
		private void OnEnable()
		{
			EventManager.OnCameraMove += OnCameraMove;
			EventManager.OnCameraZoom += OnCameraZoom;
			EventManager.OnDialogueStarted += OnDialogueStarted;
			EventManager.OnDialogueEnded += OnDialogueEnded;
		}

		private void OnDisable()
		{
			EventManager.OnCameraMove -= OnCameraMove;
			EventManager.OnCameraZoom -= OnCameraZoom;
			EventManager.OnDialogueStarted -= OnDialogueStarted;
			EventManager.OnDialogueEnded -= OnDialogueEnded;
		}

		private void Start()
		{
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
			_desiredZoom = camera.localPosition.z;
		}

		private void Update()
		{
			if (PlayerControls.InputActive == false || _movementOverridenByRoutine) return;
			
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
			if (PlayerControls.InputActive == false || _movementOverridenByRoutine) return;
			var targetPosition = targetTransform.position + offset;
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
		}

		private IEnumerator SmoothTowards(Vector3 targetPosition, Quaternion targetRotation, Vector3 cameraTargetPosition)
		{
			_movementOverridenByRoutine = true;
			var startPosition = transform.position;
			var startRotation = transform.rotation;
			var startCameraPosition = camera.localPosition;
			for (float t = 0; t < 1; t += Time.deltaTime * dialogueCameraSpeed)
			{
				transform.position = Vector3.Slerp(startPosition, targetPosition, t);
				transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
				camera.localPosition = Vector3.Slerp(startCameraPosition, cameraTargetPosition, t);
				yield return null;
			}

			_movementOverridenByRoutine = false;
		}

		private void OnDialogueStarted(CharacterBase otherCharacter)
		{
			var targetPosition = targetTransform.position;
			var relativePosition = otherCharacter.transform.position - targetPosition;
			relativePosition.y += 1.074f + 0.016f; // Player's pivot is at a different position from NPCs
			var pos = targetPosition + relativePosition * 0.5f;
			pos += dialogueOffset;
			var angle = Mathf.Atan2(relativePosition.x, relativePosition.z) * Mathf.Rad2Deg;
			// Randomize between left and right
			var randomAngle = Random.value > 0.5f ? angle - dialogueAngle : angle + dialogueAngle;
			var targetRotation = Quaternion.Euler(0, randomAngle, 0);

			_dialogueStartPosition = transform.position;
			_dialogueStartRotation = transform.rotation;
			_dialogueStartCameraPosition = camera.localPosition;

			var cameraTargetPosition = new Vector3(0, 0, -dialogueZoom);
			
			if(_smoothRoutine != null) StopCoroutine(_smoothRoutine);
			_smoothRoutine = StartCoroutine(SmoothTowards(pos, targetRotation, cameraTargetPosition));
		}

		private void OnDialogueEnded()
		{
			if(_smoothRoutine != null) StopCoroutine(_smoothRoutine);
			_smoothRoutine = StartCoroutine(SmoothTowards(_dialogueStartPosition, _dialogueStartRotation, _dialogueStartCameraPosition));
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