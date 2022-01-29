using System;
using UnityEngine;

namespace World
{
    public class Rotate : MonoBehaviour
    {
        [SerializeField] private Vector3 rotation;

        private void Update()
        {
            transform.Rotate(rotation * Time.deltaTime);
        }
    }
}