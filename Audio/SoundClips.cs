using System.Collections.Generic;
using UnityEngine;

namespace Audio
{
	[CreateAssetMenu(menuName = "eeStudio/Sound clips")]
	public class SoundClips: ScriptableObject
	{
		private static SoundClips _instance;
		private static SoundClips Instance
		{
			get
			{
				if (_instance == null)
				{
					_instance = Resources.Load<SoundClips>("SoundClips");
				}

				return _instance;
			}
		}

		[SerializeField] private AudioClip[] footStepsDirt;
		[SerializeField] private AudioClip[] footStepsRock;
		
		public static AudioClip RandomFootStepDirt => RandomSound(Instance.footStepsDirt);
		public static AudioClip RandomFootStepRock => RandomSound(Instance.footStepsRock);

		private static AudioClip RandomSound(IReadOnlyList<AudioClip> array)
		{
			return array[Random.Range(0, array.Count)];
		}
	}
}