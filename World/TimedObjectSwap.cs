using System.Collections;
using Models;
using UnityEngine;

public class TimedObjectSwap : MonoBehaviour
{
	[SerializeField] private TimeStamp onTime = new TimeStamp(20,0);
	[SerializeField] private TimeStamp offTime = new TimeStamp(7,0);

	[SerializeField] private GameObject onObject;
	[SerializeField] private GameObject offObject;

	private bool on;
	private void Start()
	{
		on = onObject.activeSelf;
	}

	private void TurnOn()
	{
		on = true;
		onObject.SetActive(true);
		offObject.SetActive(false);
	}

	private void TurnOff()
	{
		on = false;
		onObject.SetActive(false);
		offObject.SetActive(true);
	}

	private void Update()
	{
		if (TimeManager.IsBetween(onTime, offTime))
		{
			if (!on) TurnOn();
		}
		else
		{

			if (on) TurnOff();
		}
	}
}
