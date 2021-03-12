using System.Collections;
using Models;
using UnityEngine;

public class TimedLight : MonoBehaviour
{
	private Light light;

	[SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
	[SerializeField] private TimeStamp offTime = new TimeStamp(7,0);

	private float intensity;
	private bool on;
	
	private void Start()
	{
		light = GetComponent<Light>();
		intensity = light.intensity;
		on = light.enabled;
	}

	private IEnumerator TurnOn()
	{
		on = true;
		light.enabled = true;
		light.intensity = 0;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			light.intensity = Mathf.SmoothStep(0, intensity, t);
			yield return null;
		}

		light.intensity = intensity;
	}

	private IEnumerator TurnOff()
	{
		on = false;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			light.intensity = Mathf.SmoothStep(intensity, 0, t);
			yield return null;
		}
		light.enabled = false;
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
