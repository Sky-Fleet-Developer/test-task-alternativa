using System;
using UnityEngine;

namespace MyTestTask.Misc.View
{
    public class ConstantRotate : MonoBehaviour
    {
        [SerializeField] private float speed = 100f;
        private void Update()
        {
            transform.Rotate(Vector3.forward * speed * Time.deltaTime);
        }
    }
}