using WolfRPG.Character;

namespace Character
{
	public static class CharacterCustomizationController
	{
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