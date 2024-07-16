using Character;
using Interface;
using UI;
using UnityEngine;

namespace SideActivities
{
	public class Fishing: MonoBehaviour, IInteractable
	{
		public void OnCanInteract(CharacterBase characterBase)
		{
			Tooltip.Activate("Fish");
		}

		public void OnInteract(CharacterBase characterBase)
		{
		}

		public void OnEndInteract(CharacterBase characterBase)
		{
		}
	}
}