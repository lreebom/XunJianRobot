using UnityEngine;

namespace TRDev.SafetyClient
{
    public sealed class AvoidUI : MonoBehaviour
    {
        public System.Collections.Generic.List<GameObject> avoidGOList = new System.Collections.Generic.List<GameObject>();

        public LayerMask mainButtonAvoidLayerMask;

        public LayerMask otherButtonAvoidLayerMask = 1;

        private System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult> m_resultList = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();

        private UnityEngine.EventSystems.PointerEventData m_curEventData = null;

        private void Start()
        {
            m_curEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
        }

        public bool IsHitUI(Vector3 _pos)
        {
            bool l_isHit = true;

            if (m_curEventData == null)
            {
                m_curEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            }

            m_curEventData.position = _pos;

            m_resultList.Clear();

            UnityEngine.EventSystems.EventSystem.current.RaycastAll(m_curEventData, m_resultList);

            for (int i = 0; i < m_resultList.Count; i++)
            {
                int l_layerMaskValue = mainButtonAvoidLayerMask.value;

                if (l_layerMaskValue != (l_layerMaskValue | (1 << m_resultList[i].gameObject.layer)))
                {
                    if (avoidGOList.Contains(m_resultList[i].gameObject))
                    {
                        l_isHit = false;
                    }
                    break;
                }
            }

            return l_isHit;
        }

        public void IsHitUIBy2Layer(Vector3 _pos, out bool _mainIsHit, out bool _otherIsHit)
        {
            _mainIsHit = true;
            _otherIsHit = true;

            if (m_curEventData == null)
            {
                m_curEventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
            }

            m_curEventData.position = _pos;

            m_resultList.Clear();

            UnityEngine.EventSystems.EventSystem.current.RaycastAll(m_curEventData, m_resultList);

            int l_mainLayerMaskValue = mainButtonAvoidLayerMask.value;
            int l_otherLayerMaskValue = otherButtonAvoidLayerMask.value;

            bool l_isCalculateMain = false;
            bool l_isCalculateOther = false;

            System.Collections.Generic.List<float> l_mainAvoidHitCameraDepthList = new System.Collections.Generic.List<float>();
            System.Collections.Generic.List<float> l_otherAvoidHitCameraDepthList = new System.Collections.Generic.List<float>();

            for (int i = 0; i < m_resultList.Count; i++)
            {
                int l_hitLayer = m_resultList[i].gameObject.layer;

                if (!l_isCalculateMain)
                {
                    if (!Lreebom.LayerMaskContainLayer(l_mainLayerMaskValue, l_hitLayer))
                    {
                        if (m_resultList[i].module.eventCamera && !l_mainAvoidHitCameraDepthList.Contains(m_resultList[i].module.eventCamera.depth))
                        {
                            if (avoidGOList.Contains(m_resultList[i].gameObject))
                            {
                                _mainIsHit = false;
                            }

                            l_isCalculateMain = true;

                            if (l_isCalculateOther) break;
                        }
                    }
                    else
                    {
                        if (m_resultList[i].module.eventCamera)
                            l_mainAvoidHitCameraDepthList.Add(m_resultList[i].module.eventCamera.depth);
                    }
                }


                if (!l_isCalculateOther)
                {
                    if (!Lreebom.LayerMaskContainLayer(l_otherLayerMaskValue, l_hitLayer))
                    {
                        if (!l_otherAvoidHitCameraDepthList.Contains(m_resultList[i].module.eventCamera.depth))
                        {
                            if (avoidGOList.Contains(m_resultList[i].gameObject))
                            {
                                _otherIsHit = false;
                            }
                            l_isCalculateOther = true;

                            if (l_isCalculateMain) break;
                        }
                    }
                    else
                    {
                        if (m_resultList[i].module.eventCamera)
                            l_otherAvoidHitCameraDepthList.Add(m_resultList[i].module.eventCamera.depth);
                    }
                }
            }

            l_mainAvoidHitCameraDepthList.Clear();
            l_mainAvoidHitCameraDepthList = null;
            l_otherAvoidHitCameraDepthList.Clear();
            l_otherAvoidHitCameraDepthList = null;
        }
    }
}