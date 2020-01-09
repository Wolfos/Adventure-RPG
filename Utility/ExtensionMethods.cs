using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods
{
	public static Vector3 RandomPos(this Bounds bounds)
	{
		return new Vector3(
			Random.Range(bounds.min.x, bounds.max.x), 
			Random.Range(bounds.min.y, bounds.max.y), 
			Random.Range(bounds.min.z, bounds.max.z));
	}
}
