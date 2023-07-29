namespace Character
{
	public enum Gender
	{
		Female, Male
	}
	public enum CharacterCustomizationPart
	{
		Gender, Hair, Head, Eyebrows, FacialHair, Torso, ArmUpperRight, ArmUpperLeft, ArmLowerRight, ArmLowerLeft, HandRight, HandLeft, Hips, LegRight, LegLeft
	}
	
	public struct CharacterCustomizationData
	{
		public Gender Gender;
		public int Hair;
		public int Head;
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
	}
}