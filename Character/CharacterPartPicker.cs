using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Character
{
	public class CharacterPartPicker: MonoBehaviour
	{
		public Transform handSocketRight;

		[Serializable]
		private class SkinColorMaterial
		{
			public Material[] materials;
		}
		
		// Common
		[SerializeField] private GameObject[] hair;
		[SerializeField] private GameObject[] headCoverings;
		[SerializeField] private GameObject[] backAttachment;
		[SerializeField] private SkinColorMaterial[] skinColorMaterials;
		[SerializeField] public Renderer[] affectedBySkinColor;
		[SerializeField] private Renderer[] affectedByHairColor;
		[SerializeField] private Material[] hairColorMaterials;
		[SerializeField] private EyeController[] eyeControllers;
		[SerializeField] private int numEyes = 3;

		public MouthController[] mouthControllers;

		private Dictionary<CharacterCustomizationPart, GameObject> _activeParts = new();
		private bool _hairDisabled;

		#region Female
		// Female
		[SerializeField] private GameObject[] femaleHead;
		[SerializeField] private GameObject[] femaleEyebrows;
		[SerializeField] private GameObject[] femaleTorso;
		[SerializeField] private GameObject[] femaleArmUpperRight;
		[SerializeField] private GameObject[] femaleArmUpperLeft;
		[SerializeField] private GameObject[] femaleArmLowerRight;
		[SerializeField] private GameObject[] femaleArmLowerLeft;
		[SerializeField] private GameObject[] femaleHandRight;
		[SerializeField] private GameObject[] femaleHandLeft;
		[SerializeField] private GameObject[] femaleHips;
		[SerializeField] private GameObject[] femaleLegRight;
		[SerializeField] private GameObject[] femaleLegLeft;
		#endregion
		
		#region Male
		// Male
		[SerializeField] private GameObject[] maleHead;
		[SerializeField] private GameObject[] maleEyebrows;
		[SerializeField] private GameObject[] maleFacialHair;
		[SerializeField] private GameObject[] maleTorso;
		[SerializeField] private GameObject[] maleArmUpperRight;
		[SerializeField] private GameObject[] maleArmUpperLeft;
		[SerializeField] private GameObject[] maleArmLowerRight;
		[SerializeField] private GameObject[] maleArmLowerLeft;
		[SerializeField] private GameObject[] maleHandRight;
		[SerializeField] private GameObject[] maleHandLeft;
		[SerializeField] private GameObject[] maleHips;
		[SerializeField] private GameObject[] maleLegRight;
		[SerializeField] private GameObject[] maleLegLeft;
		#endregion
		
		private void DisableObjects(IEnumerable<GameObject> objects)
		{
			foreach (var obj in objects)
			{
				if(obj != null) obj.SetActive(false);
			}
		}

		public void SetSkinColor(int skinColor)
		{
			foreach (var renderer in affectedBySkinColor)
			{
				renderer.material = skinColorMaterials[skinColor].materials[0];
			}
		}
		
		public void SetHairColor(int hairColor)
		{
			foreach (var renderer in affectedByHairColor)
			{
				renderer.material = hairColorMaterials[hairColor];
			}
		}

		private void DisableFemaleObjects()
		{
			DisableObjects(femaleHead);
			DisableObjects(femaleEyebrows);
			DisableObjects(femaleTorso);
			DisableObjects(femaleArmUpperRight);
			DisableObjects(femaleArmUpperLeft);
			DisableObjects(femaleArmLowerRight);
			DisableObjects(femaleArmLowerLeft);
			DisableObjects(femaleHandRight);
			DisableObjects(femaleHandLeft);
			DisableObjects(femaleHandLeft);
			DisableObjects(femaleHips);
			DisableObjects(femaleLegRight);
			DisableObjects(femaleLegLeft);
		}

		private void DisableMaleObjects()
		{
			DisableObjects(maleHead);
			DisableObjects(maleEyebrows);
			DisableObjects(maleFacialHair);
			DisableObjects(maleTorso);
			DisableObjects(maleArmUpperRight);
			DisableObjects(maleArmUpperLeft);
			DisableObjects(maleArmLowerRight);
			DisableObjects(maleArmLowerLeft);
			DisableObjects(maleHandRight);
			DisableObjects(maleHandLeft);
			DisableObjects(maleHandLeft);
			DisableObjects(maleHips);
			DisableObjects(maleLegRight);
			DisableObjects(maleLegLeft);
		}

		public void DisableAllObjects()
		{
			_activeParts.Clear();
			
			DisableObjects(hair);
			DisableObjects(backAttachment);
			DisableFemaleObjects();
			DisableMaleObjects();
		}

		public void DisableHair()
		{
			_hairDisabled = true;
		}

		public void EnableHair()
		{
			_hairDisabled = false;
		}

		private GameObject[] PartToArray(BodyType bodyType, CharacterCustomizationPart part)
		{
			switch (part)
			{
				case CharacterCustomizationPart.BodyType:
					return null;
				case CharacterCustomizationPart.Hair:
					return hair;
				case CharacterCustomizationPart.BackAttachment:
					return backAttachment;
				case CharacterCustomizationPart.HeadCovering:
					return headCoverings;
				case CharacterCustomizationPart.Head:
					return bodyType == BodyType.Female ? femaleHead : maleHead;
				case CharacterCustomizationPart.Eyebrows:
					return bodyType == BodyType.Female ? femaleEyebrows : maleEyebrows;
				case CharacterCustomizationPart.FacialHair:
					return bodyType == BodyType.Female ? null : maleFacialHair;
				case CharacterCustomizationPart.Torso:
					return bodyType == BodyType.Female ? femaleTorso : maleTorso;
				case CharacterCustomizationPart.ArmUpperRight:
					return bodyType == BodyType.Female ? femaleArmUpperRight : maleArmUpperRight;
				case CharacterCustomizationPart.ArmUpperLeft:
					return bodyType == BodyType.Female ? femaleArmUpperLeft : maleArmUpperLeft;
				case CharacterCustomizationPart.ArmLowerRight:
					return bodyType == BodyType.Female ? femaleArmLowerRight : maleArmLowerRight;
				case CharacterCustomizationPart.ArmLowerLeft:
					return bodyType == BodyType.Female ? femaleArmLowerLeft : maleArmLowerLeft;
				case CharacterCustomizationPart.HandRight:
					return bodyType == BodyType.Female ? femaleHandRight : maleHandRight;
				case CharacterCustomizationPart.HandLeft:
					return bodyType == BodyType.Female ? femaleHandLeft : maleHandLeft;
				case CharacterCustomizationPart.Hips:
					return bodyType == BodyType.Female ? femaleHips : maleHips;
				case CharacterCustomizationPart.LegRight:
					return bodyType == BodyType.Female ? femaleLegRight : maleLegRight;
				case CharacterCustomizationPart.LegLeft:
					return bodyType == BodyType.Female ? femaleLegLeft : maleLegLeft;
				default:
					throw new ArgumentOutOfRangeException(nameof(part), part, null);
			}
		}

		public int GetNumAvailableOptions(CharacterVisualData data, CharacterCustomizationPart part)
		{
			switch (part)
			{
				case CharacterCustomizationPart.SkinColor:
					return skinColorMaterials.Length;
				case CharacterCustomizationPart.HairColor:
					return hairColorMaterials.Length;
				case CharacterCustomizationPart.Eyes:
					return numEyes;
				default:
				{
					var array = PartToArray(data.BodyType, part);
					return array?.Length ?? 0;
				}
			}
		}

		public void SetEyes(int eyes)
		{
			foreach (var ec in eyeControllers)
			{
				ec.SetEye(eyes);
			}
		}
		
		
		public void OverrideMaterials(CharacterVisualData data)
		{
			if (data.MaterialOverrides == null) return;
			
			foreach (var ovrride in data.MaterialOverrides)
			{
				if (_activeParts.TryGetValue(ovrride.Key, out var part))
				{
					var renderer = part.GetComponent<Renderer>();
					if (renderer != null && affectedBySkinColor.Contains(renderer))
					{
						renderer.material = skinColorMaterials[data.SkinColor].materials[ovrride.Value];
					}
				}
			}
		}
		
		private void EnablePart(GameObject[] array, int selectionIndex, CharacterCustomizationPart part)
		{
			var obj = array[selectionIndex];
			if (obj != null)
			{
				_activeParts.Add(part, obj);
				obj.SetActive(true);
			}
		}

		public void SelectPart(CharacterVisualData data, CharacterCustomizationPart part, int selectionIndex)
		{
			if (part == CharacterCustomizationPart.Hair && _hairDisabled)
			{
				selectionIndex = 0;
			}
			
			switch (part)
			{
				case CharacterCustomizationPart.SkinColor:
					SetSkinColor(selectionIndex);
					return;
				case CharacterCustomizationPart.HairColor:
					SetHairColor(selectionIndex);
					return;
				case CharacterCustomizationPart.Eyes:
					SetEyes(selectionIndex);
					return;
			}
			
			var array = PartToArray(data.BodyType, part);
			if (array == null) return;

			_activeParts.Remove(part);
			
			DisableObjects(array);
			EnablePart(array, selectionIndex, part);
		}
	}
}