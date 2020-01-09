using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class Tooltip : MonoBehaviour
	{
		private static Tooltip instance = null;
		private Text textDisplay;

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
			if(Input.GetMouseButtonUp(0)) gameObject.SetActive(false);
		}


		public static void Activate(string text, Vector3 position)
		{
			text = text.Replace("\\n", Environment.NewLine);
			instance.textDisplay.text = text;
			instance.gameObject.SetActive(true);
			instance.transform.position = position;
		}

		public static void DeActivate()
		{
			instance.gameObject.SetActive(false);
		}
	}
}