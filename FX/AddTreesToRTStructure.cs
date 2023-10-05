using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace FX
{
    [RequireComponent(typeof(Terrain))]
    public class AddTreesToRTStructure : MonoBehaviour
    {
        private struct TreeInstance
        {
            public int Prototype;
            public Matrix4x4 Matrix;
        }

        private struct TreePrototype
        {
            public TreeInstance[] Instances;
            public Mesh[] Meshes;
            public Material[] Materials;
        }

        public struct InstanceData
        {
            public Matrix4x4 objectToWorld;
        }

        private void Awake()
        {
            // TODO: Pretty sure this requires building the entire RT acceleration structure manually. Ouch.
            // var terrain = GetComponent<Terrain>();
            // var camera = Camera.main;
            //
            // var instances = new TreeInstance[terrain.terrainData.treeInstanceCount];
            //
            // for (var i = 0; i < terrain.terrainData.treeInstanceCount; i++)
            // {
            //     var instance = terrain.terrainData.treeInstances[i];
            //     instances[i].Prototype = instance.prototypeIndex;
            //     instances[i].Matrix = Matrix4x4.Translate(instance.position) *
            //                  Matrix4x4.Scale(new(instance.widthScale, instance.heightScale, instance.widthScale));
            // }
            //
            // var prototypes = new TreePrototype[terrain.terrainData.treePrototypes.Length];
            // for (var i = 0; i < terrain.terrainData.treePrototypes.Length; i++)
            // {
            //     var prototype = terrain.terrainData.treePrototypes[i];
            //     prototypes[i].Instances = instances.Where(x => x.Prototype == i).ToArray();
            //     var lodGroup = prototype.prefab.GetComponent<LODGroup>();
            //     var renderers = lodGroup.GetLODs()[0].renderers;
            //
            //     prototypes[i].Materials = new Material[renderers.Length];
            //     prototypes[i].Meshes = new Mesh[renderers.Length];
            //     for (var j = 0; j < renderers.Length; j++)
            //     {
            //         prototypes[i].Materials[j] = renderers[j].material;
            //         prototypes[i].Meshes[j] = renderers[j].GetComponent<MeshFilter>().sharedMesh;
            //     }
            // }
            //
            // foreach (var prototype in prototypes)
            // {
            //     for (var i = 0; i < prototype.Materials.Length; i++)
            //     {
            //         var instanceConfig = new RayTracingMeshInstanceConfig
            //         {
            //             dynamicGeometry = false,
            //             enableTriangleCulling = false,
            //             frontTriangleCounterClockwise = false,
            //             layer = 0,
            //             material = prototype.Materials[i],
            //             mesh = prototype.Meshes[i]
            //         };
            //
            //         var instanceData = new InstanceData[prototype.Instances.Length];
            //         for (var j = 0; j < prototype.Instances.Length; j++)
            //         {
            //             instanceData[j].objectToWorld = prototype.Instances[j].Matrix;
            //         }
            //
            //         RayTracingAccelerationStructure.AddInstances(instanceConfig, instanceData,
            //             prototype.Instances.Length);
            //     }
            // }

        }
    }
}