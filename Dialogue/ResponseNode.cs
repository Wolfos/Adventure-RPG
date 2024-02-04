using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using WolfRPG.Core.Localization;
using XNode;

namespace Dialogue
{
	[NodeWidth(300)]
	public class ResponseNode : Node
	{
		[Input(ShowBackingValue.Never)] public Node previous;
		[Output(instancePortList = true)] public List<string> answers = new();
		[SerializeField, HideInInspector] private List<string> guids = new();

		// Use this for initialization
		protected override void Init()
		{
			base.Init();

		}

		public List<string> GetLocalized()
		{
			var ret = new List<string>();
			foreach (var guid in guids)
			{
				ret.Add(LocalizationFile.Get(guid));
			}

			return ret;
		}

		// Return the correct value of an output port when requested
		public override object GetValue(NodePort port)
		{
			return null; // Replace this
		}
		
		[Button("Update localization")]
		public void UpdateLocalizationText()
		{
			#if UNITY_EDITOR
			LocalizationFile.Load();
			for(var i = 0; i < answers.Count; i++)
			{
				var englishText = answers[i];
				string guid;
				if (i > guids.Count - 1)
				{
					guid = Guid.NewGuid().ToString();
					guids.Add(guid);
				}
				else
				{
					guid = guids[i];
				}

				if (LocalizationFile.HasIdentifier(guid, false) == false)
				{
					LocalizationFile.AddNew(guid, englishText);
				}
				else
				{
					LocalizationFile.SetEnglishText(guid, englishText);
				}
			}

			LocalizationFile.SaveFile();
#endif
		}
	}
}