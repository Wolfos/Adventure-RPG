using UnityEngine;

namespace World
{
	[RequireComponent(typeof(AudioSource))]
	public class OccasionalRandomSound : MonoBehaviour
	{
		[SerializeField] private AudioClip[] sounds;
		[SerializeField] private Range timeRange;

		private float _nextTime;
		private AudioSource _audioSource;

		private void Awake()
		{
			_audioSource = GetComponent<AudioSource>();
		}

		private void Start()
		{
			RandomTime();
		}

		private void RandomTime()
		{
			_nextTime = Time.time + Random.Range(timeRange.start , timeRange.end);
		}

		private void Update()
		{
			if (Time.time > _nextTime)
			{
				_audioSource.PlayOneShot(sounds[Random.Range(0, sounds.Length - 1)]);
				RandomTime();
			}
		}
	}
}