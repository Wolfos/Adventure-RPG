using System.Collections;
using System.Collections.Generic;
using Player;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Button))]
	public class ConsoleButtonFixer : MonoBehaviour
	{
		private Button button;
		void Start()
		{
			button = GetComponent<Button>();
		}


		void Update()
		{
			if (EventSystem.current.currentSelectedGameObject == gameObject)
			{
				if(InputMapper.InteractionButton()) button.onClick.Invoke();
			}
		}
	}
}