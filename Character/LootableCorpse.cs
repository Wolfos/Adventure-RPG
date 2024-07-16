using System;
using Interface;
using UI;
using UnityEngine;

namespace Character
{
	public class LootableCorpse: MonoBehaviour, IInteractable
	{
		public NPC Npc { get; set; }
		public Transform trackTransform;
		public void OnCanInteract(CharacterBase characterBase)
		{
			// ReSharper disable once Unity.NoNullPropagation (not an issue here)
			Tooltip.Activate(Npc?.GetName());
		}

		public void OnInteract(CharacterBase characterBase)
		{
			ItemContainerWindow.SetData(Npc.Inventory, Npc.equipment);
			WindowManager.Open<ItemContainerWindow>();
		}

		public void OnEndInteract(CharacterBase characterBase)
		{
			Tooltip.DeActivate();
		}

		private void LateUpdate()
		{
			transform.position = trackTransform.position;
		}
	}
}