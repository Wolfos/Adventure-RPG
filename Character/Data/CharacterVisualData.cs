using System;
using System.Collections.Generic;

namespace Character
{
	public enum BodyType
	{
		Female, Male
	}
	public enum CharacterCustomizationPart
	{
		BodyType, Hair, BackAttachment, HeadCovering, Head, Eyebrows, FacialHair, Torso, ArmUpperRight, ArmUpperLeft, ArmLowerRight, ArmLowerLeft, HandRight, HandLeft, Hips, LegRight, LegLeft, SkinColor, Eyes, HairColor
	}
	
	[Serializable]
	public struct CharacterVisualData
	{
		public BodyType BodyType;
		public int Hair;
		public int BackAttachment;
		public int HeadCovering;
		public int Head;
		public int Eyes;
		public int Eyebrows;
		public int FacialHair;
		public int Torso;
		public int ArmUpperRight;
		public int ArmUpperLeft;
		public int ArmLowerRight;
		public int ArmLowerLeft;
		public int HandRight;
		public int HandLeft;
		public int Hips;
		public int LegRight;
		public int LegLeft;
		public int SkinColor;
		public int HairColor;
		

		// Computed at runtime
		public Dictionary<CharacterCustomizationPart, int> MaterialOverrides { get; set; }
	}
}