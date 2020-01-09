using System.Collections;
using UnityEngine;

public class ItemAddAnimation : MonoBehaviour
{
	private IEnumerator Start()
	{
		yield return new WaitForSeconds(0.5f);
		StartCoroutine(SizeAnim());
		for (float i = 0; i < 1; i += Time.deltaTime)
		{
			Vector3 translation = Vector3.up * Time.deltaTime * 4;
			translation *= Mathf.SmoothStep(0, 1, i);
			transform.Translate(translation, Space.World);
			yield return null;
		}
		
		
	}

	private IEnumerator SizeAnim()
	{
		yield return new WaitForSeconds(0.5f);

		Vector3 startScale = transform.localScale;
		Vector3 targetScale = transform.localScale * 1.5f;
		
		for (float i = 0; i < 1; i += Time.deltaTime * 2)
		{
			transform.localScale = Vector3.Lerp(startScale, targetScale, Mathf.SmoothStep(0, 1, i));
			yield return null;
		}
		for (float i = 0; i < 1; i += Time.deltaTime * 2)
		{
			transform.localScale = Vector3.Lerp(targetScale, Vector3.zero, Mathf.SmoothStep(0, 1, i));
			yield return null;
		}
		Destroy(gameObject);
	}
}
