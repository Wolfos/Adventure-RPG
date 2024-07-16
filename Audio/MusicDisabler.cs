using System;
using UnityEngine;

namespace Audio
{
	public class MusicDisabler: MonoBehaviour
	{
		private void Awake()
		{
			MusicManager.MusicDisabled = true;
		}

		private void OnDestroy()
		{
			MusicManager.MusicDisabled = false;
		}
	}
}