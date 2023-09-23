using System.Collections;
using Models;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimedLight : MonoBehaviour
{
	private Light _light;
	private HDAdditionalLightData _hdAdditionalLightData;

	[SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
	[SerializeField] private TimeStamp offTime = new TimeStamp(7,0);
	[SerializeField] private bool bakeShadows;

	private float intensity;
	private bool on;
	
	private void Start()
	{
		_light = GetComponent<Light>();
		_hdAdditionalLightData = GetComponent<HDAdditionalLightData>();
		intensity = _light.intensity;
		on = _light.enabled;
	}

	private IEnumerator TurnOn()
	{
		on = true;
		_light.enabled = true;
		_light.intensity = 0;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			_light.intensity = Mathf.SmoothStep(0, intensity, t);
			yield return null;
		}

		// TODO: This is broken
		if (bakeShadows)
		{
			_hdAdditionalLightData.RequestShadowMapRendering();
			bakeShadows = false; // Only need to do this once
		}
		_light.intensity = intensity;
	}

	private IEnumerator TurnOff()
	{
		on = false;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			_light.intensity = Mathf.SmoothStep(intensity, 0, t);
			yield return null;
		}
		_light.enabled = false;
	}
	
	

	public void Update()
	{
		if (TimeManager.IsBetween(onTime, offTime))
		{
			if (!on)
			{
				#if UNITY_EDITOR
				if (EditorApplication.isPlaying == false)
				{
					var light = GetComponent<Light>();
					light.enabled = true;
					return;
				}
				#endif
				StartCoroutine(TurnOn());
			}
		}
		else
		{
#if UNITY_EDITOR
			if (EditorApplication.isPlaying == false)
			{
				var light = GetComponent<Light>();
				light.enabled = false;
				return;
			}
#endif
			if (on) StartCoroutine(TurnOff());
		}
	}
}
