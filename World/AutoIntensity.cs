using UnityEngine;
using System.Collections;

public class AutoIntensity : MonoBehaviour {

	[SerializeField] private Gradient nightDayColor;

	[SerializeField] private float maxIntensity = 3f;
	[SerializeField] private float minIntensity = 0f;
	[SerializeField] private float minPoint = -0.2f;

	[SerializeField] private float maxAmbient = 1f;
	[SerializeField] private float minAmbient = 0f;
	[SerializeField] private float minAmbientPoint = -0.2f;

	[SerializeField] private AnimationCurve rotationCurve;

	[SerializeField] private Gradient nightDayFogColor;
	[SerializeField] private AnimationCurve fogDensityCurve;
	[SerializeField] private float fogScale = 1f;

	[SerializeField] private float dayAtmosphereThickness = 0.4f;
	[SerializeField] private float nightAtmosphereThickness = 0.87f;

	Light mainLight;
	Material skyMat;

	void Start () 
	{
		mainLight = GetComponentInChildren<Light>();
		skyMat = RenderSettings.skybox;
		RenderSettings.sun = mainLight;
	}

	void Update () 
	{
		float tRange = 1 - minPoint;
		float dot = Mathf.Clamp01 ((Vector3.Dot (mainLight.transform.forward, Vector3.down) - minPoint) / tRange);
		float i = ((maxIntensity - minIntensity) * dot) + minIntensity;

		mainLight.intensity = i;
		mainLight.shadowStrength = dot;

		tRange = 1 - minAmbientPoint;
		dot = Mathf.Clamp01 ((Vector3.Dot (mainLight.transform.forward, Vector3.down) - minAmbientPoint) / tRange);
		i = ((maxAmbient - minAmbient) * dot) + minAmbient;

		mainLight.color = nightDayColor.Evaluate(dot);
		RenderSettings.ambientLight = mainLight.color * i;

		RenderSettings.fogColor = nightDayFogColor.Evaluate(dot);
		RenderSettings.fogDensity = fogDensityCurve.Evaluate(dot) * fogScale;

		i = ((dayAtmosphereThickness - nightAtmosphereThickness) * dot) + nightAtmosphereThickness;
		skyMat.SetFloat ("_AtmosphereThickness", i);


		transform.rotation = Quaternion.Euler(0,0, rotationCurve.Evaluate(TimeManager.Time / 24) * 360);
	}
}