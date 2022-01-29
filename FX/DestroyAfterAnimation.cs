using System.Collections;
using UnityEngine;

namespace FX
{
    public class DestroyAfterAnimation : MonoBehaviour
    {
        [SerializeField] private AnimationClip animation;
        private IEnumerator Start()
        {
            yield return new WaitForSeconds(animation.length);
            Destroy(gameObject);
        }
    }
}
