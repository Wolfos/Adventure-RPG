using UnityEngine;

namespace Utility
{
    public class PrintBoneNames : MonoBehaviour
    {
        private void Start()
        {
            var ren = GetComponent<SkinnedMeshRenderer>();
            foreach (var bone in ren.bones)
            {
                Debug.Log(bone.name);
            }
        }
    }
}