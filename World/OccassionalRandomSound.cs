using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
	public class OccassionalRandomSound : MonoBehaviour
	{
		[SerializeField] private AudioClip[] sounds;
		[SerializeField] private Range timeRange;

		private float time;
		private float randomTime;

		private void Start()
		{
			randomTime = Random.Range(timeRange.start , timeRange.end);
		}

		private void Update()
		{
			time += Time.deltaTime;
			
			if (time > randomTime)
			{
				GetComponent<AudioSource>().PlayOneShot(sounds[Random.Range(0, sounds.Length - 1)]);
				randomTime = Random.Range(timeRange.start, timeRange.end);
				time = 0;
			}
		}
	}
}