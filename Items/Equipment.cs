using System.Collections;
using System.Collections.Generic;
using Combat;
using Character;
using UnityEngine;
using Utility;

namespace Items
{
	public class Equipment : Item
	{
		private int defaultLayer;
		private SkinnedMeshRenderer meshRenderer;

		private void Awake()
		{
			onEquipped += OnEquipped;
			onUnEquipped += OnUnEquipped;
			defaultLayer = gameObject.layer;
			meshRenderer = GetComponent<SkinnedMeshRenderer>();
		}

		private void Start()
		{
			if(IsEquipped) OnEquipped(this);
		}

		private void OnDestroy()
		{
			onEquipped -= OnEquipped;
			onUnEquipped -= OnUnEquipped;
		}

		private void OnEquipped(Item item)
		{
			gameObject.SetActive(true);
			gameObject.layer = 0;
		}

		public void SetBones(Transform rootbone, Transform[] bones)
		{
			meshRenderer.rootBone = rootbone;
			meshRenderer.bones = bones;
		}

		private void OnUnEquipped(Item item)
		{
			gameObject.SetActive(false);
			gameObject.layer = defaultLayer;
		}
	}
}