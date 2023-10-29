using System;
using Models;
using UnityEngine;


public class TimeManager
{
	private static TimeManager _instance;

	public static TimeManager Instance
	{
		get
		{
			if(_instance == null) _instance = new TimeManager(12.0f);
			return _instance;
		}
	}
	
	public const float timeScale = 0.01f;
	public float time = 12.0f;
	public static int Day { get; set; }
	
	public static float Time
	{
		get => Instance.time;
		set => Instance.time = value;
	}

	public static void UpdateTime(float interval)
	{
		Time += interval;
		if (Time > 24)
		{
			Time -= 24;
			Day++;
		}
	}

	public TimeManager(float time)
	{
		if(_instance == null) _instance = this;

		Time = time;
	}

	public static int GetHoursPassedSince(int day, int hour)
	{
		var days = Day - day;
		var hours = Mathf.FloorToInt(Time) - hour;

		return Mathf.FloorToInt(days * 24 + hours);
	}

	public static void GetTime(out int day, out int hour, out int minute)
	{
		day = Day;
		hour = Mathf.FloorToInt(Time);
		minute = Mathf.FloorToInt((Time - hour) * 60);
	}

	public static float RealTime()
	{
		float time = Time;
		if (time > 24) time -= 24;
		return time;
	}

	public static bool IsBetween(TimeStamp start, TimeStamp end)
	{
		float time = RealTime();
		if (time > end && time < start) return false;

		return true;
	}
}
