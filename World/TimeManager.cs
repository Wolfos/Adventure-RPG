using UnityEditor;
using Utility;

public class TimeManager
{
	private static TimeManager _instance;

	public static TimeManager Instance
	{
		get
		{
			if(_instance == null) _instance = new TimeManager(0);
			return _instance;
		}
	}
	
	public const float timeScale = 0.01f;
	public float _time;
	
	public static float Time
	{
		get => Instance._time;
		set => Instance._time = value;
	}

	public static void UpdateTime(float interval)
	{
		Time += interval;
		if (Time > 24) Time -= 24;
	}

	public TimeManager(float time)
	{
		if(_instance == null) _instance = this;

		Time = time;
	}

	public static float RealTime()
	{
		float time = Time + 12;
		if (time > 24) time -= 24;
		return time;
	}

	public static bool IsBetween(int start, int end)
	{
		float time = RealTime();
		if (time > end && time < start) return false;

		return true;
	}
}
