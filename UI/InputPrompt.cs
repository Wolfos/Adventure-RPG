using System;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class InputPrompt: MonoBehaviour
	{
		[SerializeField] private bool controllerOnly;
		[SerializeField] private Image image;

		private void Update()
		{
			if (controllerOnly)
			{
				if (InputMapper.UsingController && image.enabled == false)
				{
					image.enabled = true;
				}
				else if (InputMapper.UsingController == false && image.enabled)
				{
					image.enabled = false;
				}
			}
		}
	}
}