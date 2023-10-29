using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using WolfRPG.Character;

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
		[SerializeField] private GameObject[] backAttachment;
		[SerializeField] private SkinColorMaterial[] skinColorMaterials;
		[SerializeField] private Renderer[] affectedBySkinColor;
		[SerializeField] private EyeController[] eyeControllers;
		[SerializeField] private int numEyes = 3;

		private Dictionary<CharacterCustomizationPart, GameObject> _activeParts = new();

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

		private GameObject[] PartToArray(Gender gender, CharacterCustomizationPart part)
		{
			switch (part)
			{
				case CharacterCustomizationPart.Gender:
					return null;
				case CharacterCustomizationPart.Hair:
					return hair;
				case CharacterCustomizationPart.BackAttachment:
					return backAttachment;
				case CharacterCustomizationPart.Head:
					return gender == Gender.Female ? femaleHead : maleHead;
				case CharacterCustomizationPart.Eyebrows:
					return gender == Gender.Female ? femaleEyebrows : maleEyebrows;
				case CharacterCustomizationPart.FacialHair:
					return gender == Gender.Female ? null : maleFacialHair;
				case CharacterCustomizationPart.Torso:
					return gender == Gender.Female ? femaleTorso : maleTorso;
				case CharacterCustomizationPart.ArmUpperRight:
					return gender == Gender.Female ? femaleArmUpperRight : maleArmUpperRight;
				case CharacterCustomizationPart.ArmUpperLeft:
					return gender == Gender.Female ? femaleArmUpperLeft : maleArmUpperLeft;
				case CharacterCustomizationPart.ArmLowerRight:
					return gender == Gender.Female ? femaleArmLowerRight : maleArmLowerRight;
				case CharacterCustomizationPart.ArmLowerLeft:
					return gender == Gender.Female ? femaleArmLowerLeft : maleArmLowerLeft;
				case CharacterCustomizationPart.HandRight:
					return gender == Gender.Female ? femaleHandRight : maleHandRight;
				case CharacterCustomizationPart.HandLeft:
					return gender == Gender.Female ? femaleHandLeft : maleHandLeft;
				case CharacterCustomizationPart.Hips:
					return gender == Gender.Female ? femaleHips : maleHips;
				case CharacterCustomizationPart.LegRight:
					return gender == Gender.Female ? femaleLegRight : maleLegRight;
				case CharacterCustomizationPart.LegLeft:
					return gender == Gender.Female ? femaleLegLeft : maleLegLeft;
				default:
					throw new ArgumentOutOfRangeException(nameof(part), part, null);
			}
		}

		public int GetNumAvailableOptions(CharacterCustomizationData data, CharacterCustomizationPart part)
		{
			if (part == CharacterCustomizationPart.SkinColor)
			{
				return skinColorMaterials.Length;
			}

			if (part == CharacterCustomizationPart.Eyes)
			{
				return numEyes;
			}
			var array = PartToArray(data.Gender, part);
			return array?.Length ?? 0;
		}

		public void SetEyes(int eyes)
		{
			foreach (var ec in eyeControllers)
			{
				ec.SetEye(eyes);
			}
		}
		
		
		public void OverrideMaterials(CharacterCustomizationData data)
		{
			if (data.MaterialOverrides == null) return;
			
			foreach (var ovrride in data.MaterialOverrides)
			{
				if (_activeParts.TryGetValue(ovrride.Key, out var part))
				{
					var renderer = part.GetComponent<Renderer>();
					if (renderer != null)
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

		public void SelectPart(CharacterCustomizationData data, CharacterCustomizationPart part, int selectionIndex)
		{
			if (part == CharacterCustomizationPart.SkinColor)
			{
				SetSkinColor(selectionIndex);
				return;
			}
			if (part == CharacterCustomizationPart.Eyes)
			{
				SetEyes(selectionIndex);
				return;
			}
			
			var array = PartToArray(data.Gender, part);
			if (array == null) return;

			_activeParts.Remove(part);
			
			DisableObjects(array);
			EnablePart(array, selectionIndex, part);
		}
	}
}