using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TimeDisplay : MonoBehaviour
{
	private TextMeshProUGUI text;
	void Start()
	{
		text = GetComponent<TextMeshProUGUI>();
	}

	void Update()
	{
		var time = TimeManager.RealTime();
		var hours = Mathf.FloorToInt(time);
		var minutes = Mathf.FloorToInt((time - hours) * 60);

		text.text = (hours < 10 ? "0" : "") + hours + ":" + (minutes < 10 ? "0" : "") + minutes;
	}
}
