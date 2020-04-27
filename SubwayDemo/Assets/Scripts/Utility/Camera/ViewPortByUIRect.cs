using UnityEngine;

namespace TRDev.SafetyClient
{
    [ExecuteInEditMode]
    public sealed class ViewPortByUIRect : MonoBehaviour
    {
        public Camera viewPortCamera;
        /// <summary>
        /// 相机区域Rect
        /// </summary>
        public RectTransform cameraRect;

        /// <summary>
        /// UI相机
        /// </summary>
        public Camera uiCamera;

        [SerializeField]
        private Lreebom.UpdateType m_updateType = Lreebom.UpdateType.Update;

        private Vector3[] m_rect4PointWorldPos = new Vector3[4] { Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero };

        private void Start()
        {
            if (!viewPortCamera)
                viewPortCamera = GetComponent<Camera>();
            if (!uiCamera)
                uiCamera = cameraRect.GetComponentInParent<Camera>();
        }
        private void Update()
        {
            if (m_updateType == Lreebom.UpdateType.Update)
            {
                SetCameraRect(cameraRect);
            }
        }
        void LateUpdate()
        {
            if (m_updateType == Lreebom.UpdateType.LateUpdate)
            {
                SetCameraRect(cameraRect);
            }
        }

        public void UpdateCameraRect()
        {
            if (cameraRect == null || viewPortCamera == null) return;

            cameraRect.GetWorldCorners(m_rect4PointWorldPos);

            Vector2 l_bottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, m_rect4PointWorldPos[0]);
            Vector2 l_topRight = RectTransformUtility.WorldToScreenPoint(uiCamera, m_rect4PointWorldPos[2]);

            float xScale = 1f;
            if (l_bottomLeft.x < 0 || l_topRight.x > Screen.width)
            {
                xScale = (Mathf.Clamp(l_topRight.x, 0f, Screen.width) - Mathf.Clamp(l_bottomLeft.x, 0f, Screen.width)) / (l_topRight.x - l_bottomLeft.x);
            }

            float l_left = Mathf.Clamp01(l_bottomLeft.x / Screen.width);
            float l_top = Mathf.Clamp01(l_bottomLeft.y / Screen.height);
            float l_right = Mathf.Clamp01(l_topRight.x / Screen.width);
            float l_bottom = Mathf.Clamp01(l_topRight.y / Screen.height);

            float viewHeight = Mathf.Clamp01(Mathf.Abs(l_top - l_bottom));
            float finalViewHeight = viewHeight * xScale;

            Rect l_camRect = new Rect(l_left,
                                    l_top + (viewHeight - finalViewHeight) * 0.5f,
                                    Mathf.Clamp01(Mathf.Abs(l_right - l_left)),
                                    Mathf.Clamp01(Mathf.Abs(l_top - l_bottom)) * xScale);

            if (viewPortCamera.rect != l_camRect)
            {
                viewPortCamera.rect = l_camRect;
            }
        }

        public void SetCameraRect(RectTransform _cameraRect)
        {
            this.cameraRect = _cameraRect;
            UpdateCameraRect();
        }
    }
}