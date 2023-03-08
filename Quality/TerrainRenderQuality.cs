using System;
using UnityEngine;

namespace Quality
{
    [Serializable]
    public class TerrainQualitySettings
    {
        public float detailDensity = 0.5f;
    }
    [ExecuteInEditMode]
    public class TerrainRenderQuality : MonoBehaviour
    {
        public TerrainQualitySettings[] settings;
        private Terrain _terrain;

        private void Awake()
        {
            _terrain = GetComponent<Terrain>();
        }

        void Update()
        {
            var qualityLevel = QualitySettings.GetQualityLevel();
            _terrain.detailObjectDensity = settings[qualityLevel].detailDensity;
        }
    }
}