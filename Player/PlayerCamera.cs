using UnityEngine;
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
		private MeshRenderer[] disabledMeshRenderers;
		private Transform targetTransform;
		private Vector3 offset;

		private void Awake()
		{
			SystemContainer.Register(this);
		}

		private void OnDestroy()
		{
			SystemContainer.UnRegister<PlayerCamera>();
		}

		private void Start()
		{
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
			targetTransform = SystemContainer.GetSystem<PlayerCharacter>().transform;
			offset = targetTransform.position - transform.position;
		}

		private void Update()
		{
			if (disabledMeshRenderers != null)
			{
				foreach (var r in disabledMeshRenderers)
				{
					r.shadowCastingMode = ShadowCastingMode.On;
					r.gameObject.layer = 0;
				}

				disabledMeshRenderers = null;
			}

			if (PlayerCharacter.inputActive == false) return;
			
			// Rotation
			var input = InputMapper.GetCameraMovement();
			transform.Rotate(Vector3.up, -input.x * rotationSpeed);
			verticalPivot.Rotate(Vector3.right, -input.y * rotationSpeed);

			var eulerAngles = verticalPivot.rotation.eulerAngles;
			if (eulerAngles.x > 180 && eulerAngles.x < verticalClamp.start) eulerAngles.x = verticalClamp.start;
			else if (eulerAngles.x < 180 && eulerAngles.x > verticalClamp.end) eulerAngles.x = verticalClamp.end;
			eulerAngles.y = 0;
			eulerAngles.z = 0;
			verticalPivot.localRotation = Quaternion.Euler(eulerAngles);
			
			// Zoom
			var position = camera.localPosition;
			position.z += InputMapper.GetZoom() * zoomSpeed;
			position.z = Mathf.Clamp(position.z, cameraDistance.start, cameraDistance.end);
			camera.localPosition = position;

			// Hide roofs
			// var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height) / 2);
			// RaycastHit hit;
			// if (Physics.Raycast(ray, out hit))
			// {
			// 	if (hit.transform.CompareTag("Roof"))
			// 	{
			// 		disabledMeshRenderers = hit.transform.GetComponentsInChildren<MeshRenderer>();
			// 		foreach (var r in disabledMeshRenderers)
			// 		{
			// 			r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
			// 			r.gameObject.layer = 10;
			// 		}
			// 	}
			// }
		}
		
		private void LateUpdate()
		{
			var targetPosition = targetTransform.position - offset;
			transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);
		}
	}
}