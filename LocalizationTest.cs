using System;
using UnityEngine;
using WolfRPG.Core.Localization;

namespace Code
{
	public class LocalizationTest: MonoBehaviour
	{
		private void Start()
		{
			var localizedString = new LocalizedString()
			{
				Identifier = "ExampleID"
			};
			Debug.Log(localizedString.Get());
		}
	}
}