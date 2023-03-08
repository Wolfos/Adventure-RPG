using Character;
using Interface;
using UI;
using UnityEngine;

namespace SideActivities
{
	public class Fishing: MonoBehaviour, IInteractable
	{
		[SerializeField] private float tooltipOffset;
		public void OnCanInteract(CharacterBase characterBase)
		{
			Tooltip.Activate("Fish", transform, transform.up * tooltipOffset);
		}

		public void OnInteract(CharacterBase characterBase)
		{
		}

		public void OnEndInteract(CharacterBase characterBase)
		{
		}
	}
}