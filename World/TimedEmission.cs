using System.Collections;
using Models;
using UnityEngine;

public class TimedEmission : MonoBehaviour
{
	private Material _material;

	[SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
	[SerializeField] private TimeStamp offTime = new TimeStamp(7,0);

	[SerializeField] private float maxEmission = 10000;

	private static readonly int EmissiveIntensity = Shader.PropertyToID("_EmissiveIntensity");
	private bool on;
	private static readonly int UseEmissiveIntensity = Shader.PropertyToID("_UseEmissiveIntensity");


	private void Start()
	{
		_material = GetComponent<MeshRenderer>().material;
		on = _material.GetInt(UseEmissiveIntensity) == 1;
	}

	private IEnumerator TurnOn()
	{
		on = true;
		_material.SetFloat(EmissiveIntensity, 0);
		_material.SetInt(UseEmissiveIntensity, 1);
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			_material.SetFloat(EmissiveIntensity, Mathf.SmoothStep(0, maxEmission, t));
			yield return null;
		}

		_material.SetFloat(EmissiveIntensity, maxEmission);
	}

	private IEnumerator TurnOff()
	{
		on = false;
		for (float t = 0; t < 1; t += Time.deltaTime * 5)
		{
			_material.SetFloat(EmissiveIntensity, Mathf.SmoothStep(maxEmission, 0, t));
			yield return null;
		}
		_material.SetFloat(EmissiveIntensity, 0);
		_material.SetInt(UseEmissiveIntensity, 0);
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
