using UnityEngine;
using System.Collections;

namespace DSI.LB
{
    public class UILookAtCamera : MonoBehaviour
    {
        [SerializeField]
        private Camera m_mainCamera;

        [SerializeField]
        private bool isSmooth = true;

        public Camera mainCamera
        {
            get { return m_mainCamera; }
            set { m_mainCamera = value; }
        }

        private float m_speed = 10f;

        void LateUpdate()
        {
            Quaternion l_targetQua = Quaternion.LookRotation(m_mainCamera.transform.forward, m_mainCamera.transform.up);

            if (isSmooth)
                transform.rotation = Quaternion.Lerp(transform.rotation, l_targetQua, Time.deltaTime * m_speed);
            else
                transform.rotation = l_targetQua;
        }
    }
}