using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class UpdateReflectionProbe : MonoBehaviour
{
   [SerializeField] private float updateRate = 10;
   [SerializeField] private ReflectionProbe probe1, probe2;
   [SerializeField] private HDAdditionalReflectionData data1, data2, displayProbe;

   private void Start()
   {
      probe1.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
      probe2.timeSlicingMode = ReflectionProbeTimeSlicingMode.IndividualFaces;
      data1.RequestRenderNextUpdate();
      StartCoroutine(UpdateProbes());
   }

   private IEnumerator UpdateProbes()
   {
      yield return null;
      var displayTexture = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32)
      {
         dimension = TextureDimension.Cube
      };
      var renderTexture1 = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32)
      {
         dimension = TextureDimension.Cube
      };
      var renderTexture2 = new RenderTexture(128, 128, 16, RenderTextureFormat.ARGB32)
      {
         dimension = TextureDimension.Cube
      };
      
      displayProbe.SetTexture(ProbeSettings.Mode.Custom, displayTexture);
      data1.SetTexture(ProbeSettings.Mode.Realtime, renderTexture1);
      data2.SetTexture(ProbeSettings.Mode.Realtime, renderTexture2);
      
      while (true)
      {
         // Update probe 2, then slowly interpolate to it
         data2.RequestRenderNextUpdate();
         for (float t = 0; t < 1; t += Time.deltaTime / updateRate)
         {
            ReflectionProbe.BlendCubemap(renderTexture1, renderTexture2, t, displayTexture);
            yield return null;
         }
         
         // Update probe 1, then slowly interpolate to it
         data1.RequestRenderNextUpdate();
         for (float t = 0; t < 1; t += Time.deltaTime / updateRate)
         {
            ReflectionProbe.BlendCubemap(renderTexture1, renderTexture2, 1 - t, displayTexture);
            yield return null;
         }
      }
   }
}
