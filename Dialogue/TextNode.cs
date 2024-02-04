using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using WolfRPG.Core.Localization;
using XNode;

namespace Dialogue
{
	[NodeWidth(300)]
	public class TextNode : Node
	{
		[FormerlySerializedAs("text")] [TextArea] public string englishText;

		[Input(ShowBackingValue.Never)] public Node previous;

		[Output] public Node next;

		[SerializeField, HideInInspector] private string guid;
		public int animation;

		// Use this for initialization
		protected override void Init()
		{
			base.Init();

		}

		public string GetLocalized()
		{
			return LocalizationFile.Get(guid);
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
			if (string.IsNullOrEmpty(guid))
			{
				guid = Guid.NewGuid().ToString();
			}

			if (LocalizationFile.HasIdentifier(guid, false) == false)
			{
				LocalizationFile.AddNew(guid, englishText);
			}
			else
			{
				LocalizationFile.SetEnglishText(guid, englishText);
			}
			
			LocalizationFile.SaveFile();
			#endif
		}
	}
}