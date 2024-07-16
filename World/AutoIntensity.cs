using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.HighDefinition;

public class AutoIntensity : MonoBehaviour 
{
	[SerializeField] private Light sun;
	[SerializeField] private Light moon;
	[SerializeField] private HDAdditionalLightData sunLightData;
	[SerializeField] private HDAdditionalLightData moonLightData;
	[SerializeField] private AnimationCurve rotationCurve;
	[SerializeField] private float updateDuration = 0.5f;
	[SerializeField] private float updateRate = 2;
	[SerializeField] private float moonShadowIntensity = 0.8f;

	private bool _moonActive;
	
	private void Start () 
	{
		RenderSettings.sun = sun;
		
		CheckDayNight();
	}

	public void CheckDayNight()
	{
		var timePoint = TimeManager.Time + 12;
		if (timePoint > 24) timePoint -= 24;
		var rotation = rotationCurve.Evaluate(timePoint / 24) * 360;
		if (rotation is > 260 or < 100) // Enable sun shadows, disable moon
		{
			_moonActive = false;
			sunLightData.EnableShadows(true);
			sunLightData.affectsVolumetric = true;
			moonLightData.EnableShadows(false);
			moonLightData.affectsVolumetric = true;
			
		}
		else if(rotation is < 260 and > 100) // Enable moon shadows, disable sun
		{
			_moonActive = true;
			sunLightData.EnableShadows(false);
			sunLightData.affectsVolumetric = false;
			moonLightData.EnableShadows(true);
			moonLightData.affectsVolumetric = true;
		}
	}

	public void Update()
	{
		var timePoint = TimeManager.Time + 12;
		if (timePoint > 24) timePoint -= 24;
		var rotation = rotationCurve.Evaluate(timePoint / 24) * 360;
		transform.rotation = Quaternion.Euler(0,0, rotation);
		if (rotation is > 100 and < 105) // Fade moon shadows in
		{
			var l = (rotation - 100) / 5;
			moonLightData.shadowDimmer = Mathf.Lerp(0, moonShadowIntensity, l);
		}
		else if (rotation is > 255 and < 260) // Fade moon shadows out
		{
			var l = (rotation - 255) / 5;
			moonLightData.shadowDimmer = Mathf.Lerp(moonShadowIntensity, 0, l);
		}
		else // Just a safety for big time shifts
		{
			moonLightData.shadowDimmer = moonShadowIntensity;
		}

		if (_moonActive && rotation is > 260 or < 100) // Enable sun shadows, disable moon
		{
			_moonActive = false;
			sunLightData.EnableShadows(true);
			sunLightData.affectsVolumetric = true;
			moonLightData.EnableShadows(false);
			moonLightData.affectsVolumetric = true;
			
		}
		else if(_moonActive == false && rotation is < 260 and > 100) // Enable moon shadows, disable sun
		{
			_moonActive = true;
			sunLightData.EnableShadows(false);
			sunLightData.affectsVolumetric = false;
			moonLightData.EnableShadows(true);
			moonLightData.affectsVolumetric = true;
		}
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