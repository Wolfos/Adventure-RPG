using System;
using Player;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace World
{
	[ExecuteAlways]
	public class SeaWaterController : MonoBehaviour
	{
		public static WaterSurface Sea;
		[SerializeField] private WaterSurface waterSurface;
		
		private Transform _offsetTarget;
		private void OnEnable()
		{
			Init();
		}

		private void Init()
		{
			Sea = waterSurface;
#if UNITY_EDITOR
			if (Application.isPlaying == false)
			{
				_offsetTarget = SceneView.lastActiveSceneView?.camera?.transform;
				return;
			}
#endif
			var audioListener = FindAnyObjectByType<AudioListener>();
			if (audioListener != null)
			{
				_offsetTarget = FindAnyObjectByType<AudioListener>().transform;
			}
		}

		private void Update()
		{
			if (_offsetTarget == null)
			{
				Init();
				if (_offsetTarget == null) return;
			}

			var offset = new Vector2(_offsetTarget.position.x, _offsetTarget.position.z);
			waterSurface.deformationAreaOffset = offset;
			waterSurface.foamAreaOffset = offset;
			
		}
	} 
}
