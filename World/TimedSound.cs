using System.Collections;
using Models;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimedSound : MonoBehaviour
{
	private AudioSource _audioSource;
	private HDAdditionalLightData _hdAdditionalLightData;

	[SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
	[SerializeField] private TimeStamp offTime = new TimeStamp(7,0);

	private float volume;
	private bool on;
	
	private void Start()
	{
		_audioSource = GetComponent<AudioSource>();
		_hdAdditionalLightData = GetComponent<HDAdditionalLightData>();
		volume = _audioSource.volume;
		on = _audioSource.enabled;
	}

	private IEnumerator TurnOn()
	{
		on = true;
		_audioSource.enabled = true;
		_audioSource.volume = 0;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			_audioSource.volume = Mathf.SmoothStep(0, volume, t);
			yield return null;
		}
		
		_audioSource.volume = volume;
	}

	private IEnumerator TurnOff()
	{
		on = false;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			_audioSource.volume = Mathf.SmoothStep(volume, 0, t);
			yield return null;
		}
		_audioSource.enabled = false;
	}
	
	

	public void Update()
	{
		if (TimeManager.IsBetween(onTime, offTime))
		{
			if (!on)
			{
				StartCoroutine(TurnOn());
			}
		}
		else
		{
			if (on) StartCoroutine(TurnOff());
		}
	}
}
