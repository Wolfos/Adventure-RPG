using System;
using Player;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class DialogueTextDisplay : MonoBehaviour
	{
		[SerializeField] private Text textField;

		private Action<int> callback;
		private float continueTimer = 0;

		private void Update()
		{
			if (continueTimer > 0.2f && InputMapper.DialogueNext())
			{
				callback?.Invoke(0);
			}
			else if (continueTimer <= 0.2f) continueTimer += Time.unscaledDeltaTime;
		}

		public void Activate(string text, Action<int> callback)
		{
			gameObject.SetActive(true);
			textField.text = text;
			this.callback = callback;
		}

		public void DeActivate()
		{
			gameObject.SetActive(false);
		}
	}
}