using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Player
{
	public class PlayerCamera : MonoBehaviour
	{
		private MeshRenderer[] disabledMeshRenderers;
		
		private void Start()
		{
			Camera.main.depthTextureMode = DepthTextureMode.Depth;
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

			var ray = Camera.main.ScreenPointToRay(new Vector2(Screen.width, Screen.height) / 2);
			RaycastHit hit;
			if (Physics.Raycast(ray, out hit))
			{
				if (hit.transform.CompareTag("Roof"))
				{
					disabledMeshRenderers = hit.transform.GetComponentsInChildren<MeshRenderer>();
					foreach (var r in disabledMeshRenderers)
					{
						r.shadowCastingMode = ShadowCastingMode.ShadowsOnly;
						r.gameObject.layer = 10;
					}
				}
			}
		}
	}
}