using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Splines;

namespace Utility
{
    [RequireComponent(typeof(SplineContainer))]
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class RiverGenerator : MonoBehaviour
    {
        [SerializeField] private float vertexDistance = 10;
        [SerializeField] private float width = 10;
        
        private SplineContainer _spline;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;
        private Mesh _mesh;


        [Button("Generate Mesh")]
        private void GenerateMesh()
        {
            var mesh = new Mesh
            {
                name = "River"
            };

            _spline = GetComponent<SplineContainer>();
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var indices = new List<int>();
            
            var length = _spline.CalculateLength();
            for (float distance = 0; distance < length; distance += vertexDistance)
            {
                var t = Mathf.Min(distance / length, 1);

                _spline.Evaluate(t, out var p, out var d, out var u);
                var position = new Vector3(p.x, p.y, p.z);
                var direction = new Vector3(d.x, d.y, d.z);
                var up = new Vector3(u.x, u.y, u.z);

                var cross = Vector3.Cross(direction, up).normalized;
                vertices.Add(position - cross * width / 2);
                vertices.Add(position + cross * width / 2);
                
                normals.Add(up);
                normals.Add(up);
            }

            for (var i = 0; i < vertices.Count / 4; i+= 4)
            {
                indices.Add(i);
                indices.Add(i+1);
                indices.Add(i+2);
                
                indices.Add(i+1);
                indices.Add(i+2);
                indices.Add(i+3);
            }

            mesh.vertices = vertices.ToArray();
            mesh.triangles = indices.ToArray();
            mesh.normals = normals.ToArray();

            _meshFilter.sharedMesh = mesh;
        }
    }
}