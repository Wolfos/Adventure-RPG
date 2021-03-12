using System.Collections;
using Character;
using UnityEngine;

namespace World
{

	public class Examine : MonoBehaviour
	{
		[SerializeField] private string text;
		[SerializeField] private Renderer renderer;

		private bool panelVisible;
		
		private void OnInteract(CharacterBase character)
		{
			if (!panelVisible)
			{
				UI.Tooltip.Activate(text, transform, Vector3.zero);
				StartCoroutine(WasInteracted(character.transform));
			}
			else
			{
				UI.Tooltip.DeActivate();
			}

			panelVisible = !panelVisible;
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