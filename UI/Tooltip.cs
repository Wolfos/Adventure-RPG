using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Tooltip : MonoBehaviour
	{
		private static Tooltip instance = null;
		private Text textDisplay;

		private Transform target;
		private Vector3 offset;

		private void Awake()
		{
			if (instance != null)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			textDisplay = GetComponentInChildren<Text>();
			
			gameObject.SetActive(false);
		}

		private void Update()
		{
			if(Input.GetMouseButtonUp(0) || PlayerMenu.isActive || PauseMenu.isActive) gameObject.SetActive(false);
		}


		public static void Activate(string text, Transform target, Vector3 offset)
		{
			text = text.Replace("\\n", Environment.NewLine);
			instance.textDisplay.text = text;
			instance.gameObject.SetActive(true);
			instance.target = target;
			instance.offset = offset;
		}

		private void LateUpdate()
		{
			transform.position = Camera.main.WorldToScreenPoint(target.position + offset);
		}

		public static void DeActivate()
		{
			instance.gameObject.SetActive(false);
		}
	}
}