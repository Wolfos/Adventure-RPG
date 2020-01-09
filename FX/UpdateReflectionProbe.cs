using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateReflectionProbe : MonoBehaviour
{
   private IEnumerator Start()
   {
      yield return new WaitForSeconds(1);
      ReflectionProbe probe = GetComponent<ReflectionProbe>();
      while (true)
      {
         probe.RenderProbe();
         yield return new WaitForSeconds(60);
      }
   }
}
