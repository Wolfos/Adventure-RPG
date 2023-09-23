using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.AI.Navigation;
using UnityEngine;

public class TerrainTrees : MonoBehaviour
{
    [SerializeField]
    private NavMeshSurface surface;
    [SerializeField]
    private float volumeSizeOffset = 0.3f;

    [Button]
    public void Bake()
    {
        var terrains = FindObjectsByType<Terrain>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var trees = new List<GameObject>();
        
        foreach (var terrain in terrains)
        {
            var data = terrain.terrainData;

            foreach (var treeInstance in data.treeInstances)
            {
                var prefab = data.treePrototypes[treeInstance.prototypeIndex].prefab;
                var colliderPrefab = prefab.GetComponent<CapsuleCollider>();
                if (colliderPrefab == null)
                {
                    Debug.LogWarning($"Tree {prefab.name} has no capsule collider on root");
                    continue;
                }
                
                var fakeTree = new GameObject("Delete me");

                var size = data.size;
                var position = terrain.transform.position;
                fakeTree.transform.position = new Vector3(treeInstance.position.x * size.x + position.x,
                    treeInstance.position.y * size.y + position.y,
                    treeInstance.position.z * size.z + position.z);
                fakeTree.transform.localScale = prefab.transform.localScale;

                
                var capsuleCollider = fakeTree.AddComponent<CapsuleCollider>();
                capsuleCollider.center = colliderPrefab.center;
                capsuleCollider.height = colliderPrefab.height;
                capsuleCollider.direction = colliderPrefab.direction;
                capsuleCollider.radius = colliderPrefab.radius;

                trees.Add(fakeTree);
            }
        }
        
        //rebuilding navmesh
        surface.BuildNavMesh();
        //destroying the thousands of gameobjects that were created for each tree
        foreach (var tree in trees)
        {
            DestroyImmediate(tree);
        }
    }
}
