using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace FX
{
    [ExecuteInEditMode]
    public class AnimatedDecal : MonoBehaviour
    {
        private DecalProjector _decalProjector;

        private void Awake()
        {
            _decalProjector = GetComponent<DecalProjector>();
        }

        private void LateUpdate()
        {
            // Workaround for a Unity bug
            _decalProjector.fadeFactor = _decalProjector.fadeFactor;
        }
    }
}