using System.Collections;
using Character;
using Interface;
using UnityEngine;

namespace World
{

	public class Examine : MonoBehaviour, IInteractable
	{
		[SerializeField] private string text;
		[SerializeField] private Renderer renderer;

		private bool panelVisible;

		public void OnCanInteract(CharacterBase characterBase)
		{
		}

		public void OnInteract(CharacterBase character)
		{
			if (!panelVisible)
			{
				UI.Tooltip.Activate(text);
				StartCoroutine(WasInteracted(character.transform));
			}
			else
			{
				UI.Tooltip.DeActivate();
			}

			panelVisible = !panelVisible;
		}

		public void OnEndInteract(CharacterBase characterBase)
		{
		}

		private IEnumerator WasInteracted(Transform iTransform)
		{
			// Cancel when player moves
			var startPos = iTransform.position;
			while (Vector3.Distance(startPos, iTransform.position) < 1)
			{
				yield return null;
			}
			
			UI.Tooltip.DeActivate();
			panelVisible = false;
		}
	}
}