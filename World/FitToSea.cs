using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace World
{
	[ExecuteInEditMode]
	public class FitToSea : MonoBehaviour
	{
		[SerializeField] private float3 offset;
		// Internal search params
		WaterSearchParameters searchParameters = new WaterSearchParameters();
		WaterSearchResult searchResult = new WaterSearchResult();

		// Update is called once per frame
		void Update()
		{
			return;
			// Build the search parameters
			searchParameters.startPositionWS = searchResult.candidateLocationWS;
			searchParameters.targetPositionWS = gameObject.transform.position;
			searchParameters.error = 0.01f;
			searchParameters.maxIterations = 8;

			// Do the search
			if (SeaWaterController.Sea.ProjectPointOnWaterSurface(searchParameters, out searchResult))
			{
				gameObject.transform.position = searchResult.projectedPositionWS + offset;
			}
		}
	}
}