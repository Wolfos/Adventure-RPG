using System.Collections;
using Models;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

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

	private void Update()
	{
		if (TimeManager.IsBetween(onTime, offTime))
		{
			if (!on) StartCoroutine(TurnOn());
		}
		else
		{

			if (on) StartCoroutine(TurnOff());
		}
	}
}
