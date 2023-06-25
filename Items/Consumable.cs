using UnityEngine;
using System.Collections;

namespace Items
{
	[RequireComponent(typeof(Item))]
	public class Consumable : MonoBehaviour
	{
		Item item;

		void Awake()
		{
			item = GetComponent<Item>();
			item.onEquipped += Equipped;
		}

		// TODO: move this to a method in ItemBase
		IEnumerator DestroySelf()
		{
			yield return null;
			//item.container.DestroyItem(item.slot);
		}

		void Equipped(Item item)
		{
			StartCoroutine("DestroySelf"); // We need all the 'equipped' events to finish triggering before we can destroy ourselves
		}
	}
}