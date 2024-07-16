using OpenWorld;
using UnityEngine;
using World;

namespace Utility
{
    public class CameraSettings : MonoBehaviour
    {
        private static Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();

            //if (WorldStreamer.CurrentWorldSpace != WorldSpace.World)
            //{
                ToggleOcclusionCulling(false);
            //}
        }

        public static void ToggleOcclusionCulling(bool enabled)
        {
           // _camera.useOcclusionCulling = enabled;
           _camera.useOcclusionCulling = false;
        }
    }
}