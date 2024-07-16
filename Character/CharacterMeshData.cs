using UnityEngine;

namespace Character
{
	public class CharacterMeshData: ScriptableObject
	{ 
		public Mesh[] hair;
		public Mesh[] headCoverings;
		public Mesh[] backAttachment;

		#region Female
		// Female
		public Mesh[] femaleHead;
		public Mesh[] femaleEyebrows;
		public Mesh[] femaleTorso;
		public Mesh[] femaleArmUpperRight;
		public Mesh[] femaleArmUpperLeft;
		public Mesh[] femaleArmLowerRight;
		public Mesh[] femaleArmLowerLeft;
		public Mesh[] femaleHandRight;
		public Mesh[] femaleHandLeft;
		public Mesh[] femaleHips;
		public Mesh[] femaleLegRight;
		public Mesh[] femaleLegLeft;
		#endregion
		
		#region Male
		// Male
		public Mesh[] maleHead;
		public Mesh[] maleEyebrows;
		public Mesh[] maleFacialHair;
		public Mesh[] maleTorso;
		public Mesh[] maleArmUpperRight;
		public Mesh[] maleArmUpperLeft;
		public Mesh[] maleArmLowerRight;
		public Mesh[] maleArmLowerLeft;
		public Mesh[] maleHandRight;
		public Mesh[] maleHandLeft;
		public Mesh[] maleHips;
		public Mesh[] maleLegRight;
		public Mesh[] maleLegLeft;
		#endregion
	}
}