using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FX
{
    [Serializable]
    public struct LOD
    {
        public Mesh[] meshes;
        public Material[] materials;
    }
    
    public struct TreeInstanceData
    {
        public Matrix4x4 objectToWorld;
    }

    
    [Serializable]
    public class TreePrefab
    {
        public GameObject prefab;
        public float renderDistance;
        public float lodDistance;
        public LOD lod0;
        public LOD lod1;
        public Vector3 scale;
        [NonSerialized]public TreeInstanceData[] l0;
        [NonSerialized]public TreeInstanceData[] l1;
        [NonSerialized]public int lod0Count;
        [NonSerialized]public int lod1Count;
    }

    public class TreeRenderer : MonoBehaviour
    {
        [SerializeField] private TreePrefab[] prefabs;
        [SerializeField] private Terrain[] terrains;
        private const int MaxLod0 = 10000;
        private const int MaxLod1 = 50000;
        
        private struct TreeInstance
        {
            public Vector3 position;
            public TreePrefab prefab;
            public TreeInstanceData InstanceData;
        }
        
        private TreeInstance[] _treeInstances;
        private Transform _cameraTransform;

        private void Awake()
        {
            _cameraTransform = Camera.main.transform;

            foreach (var terrain in terrains)
            {
                terrain.drawTreesAndFoliage = false;
            }
            foreach (var prefab in prefabs)
            {
                prefab.l0 = new TreeInstanceData[MaxLod0];
                prefab.l1 = new TreeInstanceData[MaxLod1];
            }
            BakeInstances();
        }

        private void BakeInstances()
        {
            var treeInstances = new List<TreeInstance>();
            foreach (var terrain in terrains)
            {
                var terrainData = terrain.terrainData;
                foreach (var ins in terrainData.treeInstances)
                {
                    var prototype = terrainData.treePrototypes[ins.prototypeIndex];
                    var prefab = prefabs.First(p => p.prefab == prototype.prefab);
                    var objectToWorld = Matrix4x4.Translate(ins.position) *
                                        Matrix4x4.Scale(prefab.scale * ins.heightScale);
                    var treeInstance = new TreeInstance
                    {
                        position = ins.position,
                        InstanceData = new()
                        {
                            objectToWorld = objectToWorld
                        },
                        prefab = prefab
                    };
                    treeInstances.Add(treeInstance);
                }
            }

            _treeInstances = treeInstances.ToArray();
        }

        [Button("Bake")]
        public void FillData()
        {
            terrains = FindObjectsByType<Terrain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            
            var prefabList = prefabs == null ? new() : prefabs.ToList();
            foreach (var terrain in terrains)
            {
                foreach (var prototype in terrain.terrainData.treePrototypes)
                {
                    var prefab = prototype.prefab;
                    if (prefabList.Any(p => p.prefab == prefab) == false)
                    {
                        var lodGroup = prefab.GetComponent<LODGroup>();
                        var treePrefab = new TreePrefab
                        {
                            prefab = prefab,
                            renderDistance = 1000,
                            lodDistance = 100,
                            scale = prefab.transform.localScale
                        };
            
                        var lods = lodGroup.GetLODs();
                        var lod = lods[0];
                        
                        treePrefab.lod0.meshes = new Mesh[lod.renderers.Length];
                        treePrefab.lod0.materials = new Material[lod.renderers.Length];
                        for (var i = 0; i < lod.renderers.Length; i++)
                        {
                            treePrefab.lod0.meshes[i] = lod.renderers[i].GetComponent<MeshFilter>().sharedMesh;
                            treePrefab.lod0.materials[i] = lod.renderers[i].sharedMaterial;
                        }

                        if(lods.Length > 1)
                        {
                            lod = lods[1];
                            treePrefab.lod1.meshes = new Mesh[lod.renderers.Length];
                            treePrefab.lod1.materials = new Material[lod.renderers.Length];
                            for (var i = 0; i < lod.renderers.Length; i++)
                            {
                                treePrefab.lod1.meshes[i] = lod.renderers[i].GetComponent<MeshFilter>().sharedMesh;
                                treePrefab.lod1.materials[i] = lod.renderers[i].sharedMaterial;
                            }
                        }

                        
                        prefabList.Add(treePrefab);
                    }
                }
            }

            prefabs = prefabList.ToArray();
        }

        public void SetInstances()
        {
            var cameraPosition = _cameraTransform.position;
            foreach (var prefab in prefabs)
            {
                prefab.lod0Count = 0;
                prefab.lod1Count = 0;
            }
            foreach (var instance in _treeInstances)
            {
                var squareDistance = (instance.position - cameraPosition).sqrMagnitude;
                if (squareDistance > instance.prefab.renderDistance * instance.prefab.renderDistance)
                {
                    if (squareDistance > instance.prefab.lodDistance * instance.prefab.lodDistance)
                    {
                        instance.prefab.l0[instance.prefab.lod0Count] = instance.InstanceData;
                        instance.prefab.lod0Count++;
                    }
                    else
                    {
                        instance.prefab.l1[instance.prefab.lod1Count] = instance.InstanceData;
                        instance.prefab.lod1Count++;
                    }
                }
            }
            
        }

        public void RenderInstances()
        {
            for (var p = 0; p < prefabs.Length; p++)
            {
                var prefab = prefabs[p];
                for (var m = 0; m < prefab.lod0.meshes.Length; m++)
                {
                    var mesh = prefab.lod0.meshes[m];
                    var material = prefab.lod0.materials[m];
                    var rp = new RenderParams(material);
                    Graphics.RenderMeshInstanced(rp, mesh, 0, prefab.l0, prefab.lod0Count);
                }
                for (var m = 0; m < prefab.lod1.meshes.Length; m++)
                {
                    var mesh = prefab.lod1.meshes[m];
                    var material = prefab.lod1.materials[m];
                    var rp = new RenderParams(material);
                    Graphics.RenderMeshInstanced(rp, mesh, 0, prefab.l1, prefab.lod1Count);
                }
  
            }
        }

        public void Update()
        {
            SetInstances();
            RenderInstances();
        }
    }
}