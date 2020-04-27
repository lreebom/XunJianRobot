using UnityEngine;

namespace TRDev.SafetyClient
{
    public sealed class MinWidthScaler : MonoBehaviour
    {
        private RectTransform m_toolbarRT;
        private RectTransform m_parentRT;

        private int m_lastWidth = 0;
        private int m_lastParentWidth = 0;

        [SerializeField]
        private float m_jianGe = 20f;

        void Start()
        {
            m_toolbarRT = transform as RectTransform;

            m_parentRT = transform.parent as RectTransform;

            m_lastParentWidth = (int)m_parentRT.rect.width;

            m_lastWidth = (int)m_toolbarRT.sizeDelta.x;

            UpdateScale(m_lastParentWidth);
        }

        void LateUpdate()
        {
            int l_curParentWidth = (int)m_parentRT.rect.width;
            int l_curWidth = (int)m_toolbarRT.sizeDelta.x;

            if (m_lastParentWidth != l_curParentWidth || m_lastWidth != l_curWidth)
            {
                UpdateScale(l_curParentWidth);

                m_lastParentWidth = l_curParentWidth;

                m_lastWidth = l_curWidth;
            }
        }

        void UpdateScale(int _parentWidth)
        {
            if (_parentWidth >= m_toolbarRT.sizeDelta.x + m_jianGe)
            {
                m_toolbarRT.localScale = Vector3.one;
            }
            else
            {
                m_toolbarRT.localScale = Vector3.one * (Mathf.InverseLerp(0f, m_toolbarRT.sizeDelta.x + m_jianGe, _parentWidth));
            }
        }

        public float GetTargetScale(float width)
        {
            float parentWidth = m_parentRT.rect.width;

            if (parentWidth >= m_toolbarRT.sizeDelta.x + m_jianGe)
            {
                return 1f;
            }
            else
            {
                return (Mathf.InverseLerp(0f, width + m_jianGe, parentWidth));
            }
        }
    }
}