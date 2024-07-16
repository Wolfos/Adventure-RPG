using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utility
{
    public class MeshCombiner : MonoBehaviour
    {
        [SerializeField, HideInInspector] private MeshRenderer[] meshRenderers;
        [SerializeField, HideInInspector] private List<GameObject> combinedObjects = new();
        
        #if UNITY_EDITOR
        [Button("Combine meshes")]
        public void CombineMeshes()
        {
            UndoCombine();
            
            meshRenderers = GetComponentsInChildren<MeshRenderer>().Where(m => m.enabled && m.gameObject.activeInHierarchy && m.gameObject.isStatic).ToArray();

            Dictionary<Material, List<MeshFilter>> filtersByMaterial = new();

            foreach (var meshRenderer in meshRenderers)
            {
                if(meshRenderer.sharedMaterials.Length > 1) continue;
                var material = meshRenderer.sharedMaterial;

                if (filtersByMaterial.ContainsKey(material) == false)
                {
                    filtersByMaterial.Add(material, new());
                }

                var list = filtersByMaterial[material];
                list.Add(meshRenderer.GetComponent<MeshFilter>());
            }

            Matrix4x4 GetParentRelativeMatrix(Transform childObject, Transform parentObject)
            {
                return parentObject.worldToLocalMatrix * childObject.localToWorldMatrix;
            }

            foreach (var kvp in filtersByMaterial)
            {
                var material = kvp.Key;
                var meshFilters = kvp.Value;
                var combine = new CombineInstance[meshFilters.Count];
                for (var i = 0; i < combine.Length; i++)
                {
                    combine[i].mesh = meshFilters[i].sharedMesh;
                    combine[i].transform = GetParentRelativeMatrix(meshFilters[i].transform, transform);
                    meshFilters[i].GetComponent<MeshRenderer>().enabled = false;
                }
                
                var combinedMesh = new Mesh();
                combinedMesh.indexFormat = IndexFormat.UInt32;
                combinedMesh.CombineMeshes(combine);
                var go = new GameObject(material.name);
                go.transform.parent = transform;
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
                go.transform.localScale = Vector3.one;

                var savedMesh = SaveMesh(combinedMesh, gameObject.name + "-" + material.name, true, true);
                go.AddComponent<MeshFilter>().sharedMesh = savedMesh;
                go.AddComponent<MeshRenderer>().material = material;
                go.isStatic = true;
                
                combinedObjects.Add(go);
            }
        }

        [Button("Undo")] 
        public void UndoCombine()
        {
            if (meshRenderers == null) return;

            foreach (var meshRenderer in meshRenderers)
            {
                meshRenderer.enabled = true;
            }

            foreach (var go in combinedObjects)
            {
                DestroyImmediate(go);
            }
            combinedObjects.Clear();

            meshRenderers = null;
        }
        
        private static Mesh SaveMesh (Mesh mesh, string name, bool makeNewInstance, bool optimizeMesh)
        {
            var path = "Assets/CombinedMeshes/" + name + ".asset";

            Mesh meshToSave = (makeNewInstance) ? Object.Instantiate(mesh) as Mesh : mesh;
		
            if (optimizeMesh)
                MeshUtility.Optimize(meshToSave);
        
            AssetDatabase.CreateAsset(meshToSave, path);
            AssetDatabase.SaveAssets();

            return meshToSave;
        }
        #endif
    }
}