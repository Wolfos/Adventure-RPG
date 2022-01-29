using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour
{
	private Text text;
	void Start()
	{
		text = GetComponent<Text>();
	}

	void Update()
	{
		var time = TimeManager.RealTime();
		var hours = Mathf.FloorToInt(time);
		var minutes = Mathf.FloorToInt((time - hours) * 60);

		text.text = (hours < 10 ? 0.ToString() : "") + hours + ":" + (minutes < 10 ? 0.ToString() : "") + minutes;
	}
}
