using Character;
using Interface;
using UI;
using UnityEngine;

namespace World
{
	public class InteractionProxy: MonoBehaviour, IInteractable
	{
		[SerializeField] private GameObject interactionObject;
		public void OnCanInteract(CharacterBase characterBase)
		{
			// TODO: Localize
			Tooltip.Activate("Use");
		}

		public void OnInteract(CharacterBase characterBase)
		{
			interactionObject.GetComponent<IInteractable>().OnInteract(characterBase);
		}

		public void OnEndInteract(CharacterBase characterBase)
		{
			Tooltip.DeActivate();
		}
	}
}