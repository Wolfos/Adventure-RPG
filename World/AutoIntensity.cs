using UnityEngine;
using System.Collections;

public class AutoIntensity : MonoBehaviour 
{
	[SerializeField] private Light sun;
	[SerializeField] private AnimationCurve rotationCurve;

	private void Start () 
	{
		RenderSettings.sun = sun;
	}

	public void Update()
	{
		var timePoint = TimeManager.Time + 12;
		if (timePoint > 24) timePoint -= 24;
		transform.rotation = Quaternion.Euler(0,0, rotationCurve.Evaluate(timePoint / 24) * 360);
	}
}