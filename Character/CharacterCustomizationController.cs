using System;
using WolfRPG.Character;

namespace Character
{
	public static class CharacterCustomizationController
	{
		public static void SetPart(CharacterCustomizationPart part, ref CharacterCustomizationData data, int value)
		{
			switch (part)
			{
				case CharacterCustomizationPart.Gender:
					data.Gender = (Gender)value;
					break;
				case CharacterCustomizationPart.Hair:
					data.Hair = value;
					break;
				case CharacterCustomizationPart.BackAttachment:
					data.BackAttachment = value;
					break;
				case CharacterCustomizationPart.Head:
					data.Head = value;
					break;
				case CharacterCustomizationPart.Eyebrows:
					data.Eyebrows = value;
					break;
				case CharacterCustomizationPart.FacialHair:
					data.FacialHair = value;
					break;
				case CharacterCustomizationPart.Torso:
					data.Torso = value;
					break;
				case CharacterCustomizationPart.ArmUpperRight:
					data.ArmUpperRight = value;
					break;
				case CharacterCustomizationPart.ArmUpperLeft:
					data.ArmUpperLeft = value;
					break;
				case CharacterCustomizationPart.ArmLowerRight:
					data.ArmLowerRight = value;
					break;
				case CharacterCustomizationPart.ArmLowerLeft:
					data.ArmLowerLeft = value;
					break;
				case CharacterCustomizationPart.HandRight:
					data.HandRight = value;
					break;
				case CharacterCustomizationPart.HandLeft:
					data.HandLeft = value;
					break;
				case CharacterCustomizationPart.Hips:
					data.Hips = value;
					break;
				case CharacterCustomizationPart.LegRight:
					data.LegRight = value;
					break;
				case CharacterCustomizationPart.LegLeft:
					data.LegLeft = value;
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(part), part, null);
			}
		}
		public static void SetData(CharacterCustomizationData data, CharacterPartPicker partPicker)
		{
			partPicker.DisableAllObjects();
			
			partPicker.SelectPart(data, CharacterCustomizationPart.Hair, data.Hair);
			partPicker.SelectPart(data, CharacterCustomizationPart.BackAttachment, data.BackAttachment);
			partPicker.SelectPart(data, CharacterCustomizationPart.Head, data.Head);
			partPicker.SelectPart(data, CharacterCustomizationPart.Eyebrows, data.Eyebrows);
			partPicker.SelectPart(data, CharacterCustomizationPart.FacialHair, data.FacialHair);
			partPicker.SelectPart(data, CharacterCustomizationPart.Torso, data.Torso);
			partPicker.SelectPart(data, CharacterCustomizationPart.ArmUpperRight, data.ArmUpperRight);
			partPicker.SelectPart(data, CharacterCustomizationPart.ArmUpperLeft, data.ArmUpperLeft);
			partPicker.SelectPart(data, CharacterCustomizationPart.ArmLowerRight, data.ArmLowerRight);
			partPicker.SelectPart(data, CharacterCustomizationPart.ArmLowerLeft, data.ArmLowerLeft);
			partPicker.SelectPart(data, CharacterCustomizationPart.HandRight, data.HandRight);
			partPicker.SelectPart(data, CharacterCustomizationPart.HandLeft, data.HandLeft);
			partPicker.SelectPart(data, CharacterCustomizationPart.Hips, data.Hips);
			partPicker.SelectPart(data, CharacterCustomizationPart.LegRight, data.LegRight);
			partPicker.SelectPart(data, CharacterCustomizationPart.LegLeft, data.LegLeft);
		}
	}
}