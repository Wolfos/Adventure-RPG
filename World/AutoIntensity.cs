using UnityEngine;
using System.Collections;

public class AutoIntensity : MonoBehaviour 
{
	[SerializeField] private Light sun;
	[SerializeField] private AnimationCurve rotationCurve;
	[SerializeField] private float updateDuration = 0.5f;
	[SerializeField] private float updateRate = 2;

	private void Start () 
	{
		RenderSettings.sun = sun;
		//StartCoroutine(UpdateLightRotation());
	}

	public void Update()
	{
		var timePoint = TimeManager.Time + 12;
		if (timePoint > 24) timePoint -= 24;
		transform.rotation = Quaternion.Euler(0,0, rotationCurve.Evaluate(timePoint / 24) * 360);
	}

	// This reduces shadow shimmering but it looks pretty bad during sunrise / sunset
	// private IEnumerator UpdateLightRotation()
	// {
	// 	while (true)
	// 	{
	// 		var timePoint = TimeManager.Time + 12;
	// 		if (timePoint > 24) timePoint -= 24;
	// 		var oldRotation = transform.rotation;
	// 		var newRotation = Quaternion.Euler(0,0, rotationCurve.Evaluate(timePoint / 24) * 360);
	// 		for (float t = 0; t < 1; t += Time.deltaTime / updateDuration)
	// 		{
	// 			transform.rotation = Quaternion.Slerp(oldRotation, newRotation, t);
	// 			yield return null;
	// 		}
	// 		
	// 		yield return new WaitForSeconds(updateRate);
	// 	}
	// }
}