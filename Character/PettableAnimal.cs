using Interface;
using UI;
using UnityEngine;
using WolfRPG.Core.Localization;

namespace Character
{
	public class PettableAnimal: MonoBehaviour, IInteractable
	{
		[SerializeField] private float tooltipOffset = 2;
		[SerializeField] private float animalHeight = 0;
		[SerializeField] private float strokeWidth = 0.2f;
		[SerializeField] private float pettingDistance = 0.3f;
		[SerializeField] private LocalizedString toolTip;

		private const float AnimationLength = 10.1f;
		
		public void OnCanInteract(CharacterBase characterBase)
		{
			Tooltip.Activate(toolTip.Get(), transform, transform.up * tooltipOffset); 
		}

		public void OnInteract(CharacterBase characterBase)
		{
			characterBase.PetAnimal(strokeWidth, animalHeight, AnimationLength);
			var characterTransform = characterBase.transform;
			var pettingPosition = characterTransform.position + characterBase.graphic.forward * pettingDistance;
			GetComponent<NPC>().WalkToAndStop(pettingPosition, AnimationLength);
		}

		public void OnEndInteract(CharacterBase characterBase)
		{
			Tooltip.DeActivate();
		}
		
		private void OnDestroy()
		{
			Tooltip.DeActivate();
		}
	}
}