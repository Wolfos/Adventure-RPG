using System.Collections;
using UnityEngine;

namespace Character
{
    public class MouthController : MonoBehaviour
    {
        [SerializeField] private MeshRenderer mouthMesh;
        [SerializeField] private int framesX = 4;
        [SerializeField] private int framesY = 4;
        [SerializeField] private int closedFrame = 0;
        [SerializeField] private int openFrame = 1;

        private Material _mouthMaterial;

        private void Awake()
        {
            _mouthMaterial = mouthMesh.material;
        }

        private Vector2 GetOffset(int frame)
        {
            float y = frame / framesX;
            float x = frame - y / framesY;
			
            return new(x / framesX, y / framesY);
        }

        private void SetFrame(int frame)
        {
            _mouthMaterial.mainTextureOffset = GetOffset(frame);
        }

        public void OpenMouth()
        {
            SetFrame(openFrame);
        }

        public void CloseMouth()
        {
            SetFrame(closedFrame);
        }


    }
}