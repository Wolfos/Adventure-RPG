using System.Collections;
using UnityEngine;

namespace World
{
	public class AudioTrigger : MonoBehaviour
	{
		[SerializeField] private bool fadeIn, fadeOut;
		private AudioSource audioSource;
		[SerializeField] private float volume = 1;
		[SerializeField] private float fadeSpeed = 0.5f;

		private void Start()
		{
			audioSource = GetComponent<AudioSource>();
		}
		
		private void OnTriggerEnter(Collider other)
		{
			if (other.tag == "LocalPlayer")
			{
				if (fadeIn) StartCoroutine(FadeIn());
				else audioSource.volume = 1;
				audioSource.Play();
			}
		}

		private void OnTriggerExit(Collider other)
		{
			if (fadeOut && audioSource.isPlaying && other.tag == "LocalPlayer")
			{
				StartCoroutine(FadeOut());
			}
		}
		
		private IEnumerator FadeIn()
		{
			for (float t = 0; t < 1; t += Time.deltaTime * fadeSpeed)
			{
				audioSource.volume = Mathf.Lerp(0, volume, t);
				yield return null;
			}

			audioSource.volume = volume;
		}

		private IEnumerator FadeOut()
		{
			for (float t = 1; t > 0; t -= Time.deltaTime * fadeSpeed)
			{
				audioSource.volume = Mathf.Lerp(0, volume, t);
				yield return null;
			}

			audioSource.Stop();
		}
	}
}