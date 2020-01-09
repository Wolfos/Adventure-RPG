using System.Collections;
using NPC;
using UnityEngine;

namespace World
{

	public class Examine : MonoBehaviour
	{
		[SerializeField] private string text;
		[SerializeField] private Renderer renderer;

		private bool panelVisible;

		private void OnMouseOver()
		{
			panelVisible = true;
			UI.Tooltip.Activate(text, Input.mousePosition);
			renderer.material.SetColor("_EmissionColor", Color.grey);
		}

		private void OnMouseExit()
		{
			panelVisible = true;
			UI.Tooltip.DeActivate();
			renderer.material.SetColor("_EmissionColor", Color.black);
		}

		private void OnInteract(Character character)
		{
			if (!panelVisible)
			{
				var position = Camera.main.WorldToScreenPoint(transform.position);
				UI.Tooltip.Activate(text, position);
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