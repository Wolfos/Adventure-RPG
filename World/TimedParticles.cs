using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;

namespace World
{
	public class TimedParticles : MonoBehaviour
	{
		private ParticleSystem particles;

		[SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
		[SerializeField] private TimeStamp offTime = new TimeStamp(6, 0);

		private ParticleSystem.EmissionModule emission;
		private float startEmission;
		private bool on;

		private void Start()
		{
			particles = GetComponent<ParticleSystem>();
			emission = particles.emission;
			startEmission = emission.rateOverTimeMultiplier;
			on = true;
		}

		private IEnumerator TurnOn()
		{
			on = true;

			for (float t = 0; t < 1; t += Time.deltaTime * 5)
			{
				emission.rateOverTimeMultiplier = Mathf.SmoothStep(0, startEmission, t);
				yield return null;
			}

			emission.rateOverTimeMultiplier = startEmission;
		}

		private IEnumerator TurnOff()
		{
			on = false;
			yield return null;
			emission.rateOverTimeMultiplier = 0;
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
}