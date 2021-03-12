using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class AutoIntensity : MonoBehaviour {

	[SerializeField] private Gradient nightDayColor;

	[SerializeField] private float maxIntensity = 3f;
	[SerializeField] private float minIntensity = 0f;
	[SerializeField] private float minPoint = -0.2f;

	[SerializeField] private float maxAmbient = 1f;
	[SerializeField] private float minAmbient = 0f;
	[SerializeField] private float minAmbientPoint = -0.2f;

	[SerializeField] private AnimationCurve rotationCurve;
	[SerializeField] private AnimationCurve exposureCurve;
	[SerializeField] private VolumeProfile volumeProfile;

	private Exposure exposureComponent;

	Light mainLight;
	Material skyMat;

	void Start () 
	{
		mainLight = GetComponentInChildren<Light>();
		skyMat = RenderSettings.skybox;
		RenderSettings.sun = mainLight;
		volumeProfile.TryGet<Exposure>(out exposureComponent);
	}

	void Update () 
	{
		float tRange = 1 - minPoint;
		float dot = Mathf.Clamp01 ((Vector3.Dot (mainLight.transform.forward, Vector3.down) - minPoint) / tRange);
		float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

		//mainLight.intensity = i;
		//mainLight.shadowStrength = dot;

		tRange = 1 - minAmbientPoint;
		dot = Mathf.Clamp01 ((Vector3.Dot (mainLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
		i = ((maxAmbient - minAmbient) * dot) + minAmbient;

		//mainLight.color = nightDayColor.Evaluate(dot);
		//RenderSettings.ambientLight = mainLight.color * i;

		//exposureComponent.fixedExposure.value = exposureCurve.Evaluate(TimeManager.Time / 24) * 11;
		var timePoint = TimeManager.Time + 12;
		if (timePoint > 24) timePoint -= 24;
		transform.rotation = Quaternion.Euler(0,0, rotationCurve.Evaluate(timePoint / 24) * 360);
	}
}