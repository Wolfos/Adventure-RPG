using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using Random = UnityEngine.Random;

namespace Character
{
	public class EyeController : MonoBehaviour
	{
		[SerializeField] private MeshRenderer leftEye;
		[SerializeField] private MeshRenderer rightEye;

		[SerializeField] private int framesX = 4;
		[SerializeField] private int framesY = 4;

		[SerializeField] private int blinkFrame = 0;
		public int defaultFrame = 1;
		[SerializeField] private float eyeHue;

		[SerializeField] private float blinkTimeMin = 3;
		[SerializeField] private float blinkTimeMax = 10;

		private Material _leftMaterial;
		private Material _rightMaterial;
		private float _nextBlinkTime;
		private float _blinkEndTime;

		private void Awake()
		{
			// ReSharper disable once ReplaceWithSingleAssignment.False
			bool isEditor = false;
			
			#if UNITY_EDITOR
			if (EditorApplication.isPlaying == false) isEditor = true;
			#endif

			if (isEditor)
			{
				_leftMaterial = leftEye.sharedMaterial;
				_rightMaterial = rightEye.sharedMaterial;
			}
			else
			{
				_leftMaterial = leftEye.material;
				_rightMaterial = rightEye.material;
			}

			_nextBlinkTime = Time.time + Random.Range(blinkTimeMin, blinkTimeMax);
			_blinkEndTime = _nextBlinkTime + 0.1f;
		}

		public void SetEye(int eye)
		{
			Awake();
			
			defaultFrame = eye;
			_leftMaterial.mainTextureOffset = GetOffset(defaultFrame);
			_rightMaterial.mainTextureOffset = GetOffset(defaultFrame);
		}

		private void Update()
		{
			if (Time.unscaledTime > _nextBlinkTime && 
			    Time.unscaledTime < _blinkEndTime) // Blinking
			{
				_leftMaterial.mainTextureOffset = GetOffset(blinkFrame);
				_rightMaterial.mainTextureOffset = GetOffset(blinkFrame);
			}
			else if (Time.unscaledTime > _nextBlinkTime) // End blink
			{
				_nextBlinkTime = Time.unscaledTime + Random.Range(blinkTimeMin, blinkTimeMax);
				_blinkEndTime = _nextBlinkTime + 0.1f;
			}
			else // Not blinking
			{
				_leftMaterial.mainTextureOffset = GetOffset(defaultFrame);
				_rightMaterial.mainTextureOffset = GetOffset(defaultFrame);
			}
		}
		

		private Vector2 GetOffset(int frame)
		{
			float y = frame / framesX;
			float x = frame - y / framesY;
			
			return new(x / framesX, y / framesY);
		}
	}
}