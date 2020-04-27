using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TRDev.SafetyClient.YiQing
{
    public class YQ_CameraFollow : MonoBehaviour
    {
        public Transform cameraCenter;
        public Transform cameraTarget;

        public float followSpeed = 1f;

        private void Start()
        {

        }

        private void Update()
        {
            cameraCenter.position = cameraTarget.position;
        }

        private void LateUpdate()
        {
            //cameraCenter.position = Vector3.Lerp(cameraCenter.position, cameraTarget.position, Time.deltaTime * followSpeed);
        }
    }
}