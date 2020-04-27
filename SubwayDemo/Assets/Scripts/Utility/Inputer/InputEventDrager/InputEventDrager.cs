using UnityEngine;
using UnityEngine.EventSystems;

namespace TRDev.SafetyClient
{
    [RequireComponent(typeof(InputEventer))]
    public sealed class InputEventDrager : MonoBehaviour
    {
        /// <summary>
        /// 拖拽类型
        /// </summary>
        public DragType dragType = DragType.FreedomMove;

        public bool useRigibody = false;

        /// <summary>
        /// 拖拽的轴
        /// </summary>
        public Vector3 dragAxis = Vector3.right;

        /// <summary>
        /// 拖拽平面的法线
        /// </summary>
        public Vector3 dragPlaneNormal = Vector3.up;

        /// <summary>
        /// 拖拽速度
        /// </summary>
        public float dragSpeed = 1f;

        /// <summary>
        /// 动态旋转方向
        /// </summary>
        public bool dynamicRotateDir = false;

        public class OnDragEvent : UnityEngine.Events.UnityEvent<Transform> { }
        public OnDragEvent onStartDrag = new OnDragEvent();
        public OnDragEvent onDrag = new OnDragEvent();
        public OnDragEvent onEndDrag = new OnDragEvent();

        private InputEventer m_inputEventer;

        private Transform m_dragTarget = null;

        private float m_dragRatio = 1f;
        private Plane m_dragPlane;
        private Vector3 m_offsetWorldPos;
        private Vector2 m_dragRotDir;

        private Vector3 m_dragRotLocalPos;

        private Rigidbody m_dragRgbd = null;
        private Vector3 m_dragPos = Vector3.zero;

        private Rigidbody curRigidbody
        {
            get
            {
                if (m_dragRgbd == null)
                    m_dragRgbd = m_dragTarget.GetComponent<Rigidbody>();
                return m_dragRgbd;
            }
        }

        private bool isMove { get { return (dragType == DragType.FreedomMove || dragType == DragType.SelfAxisMove || dragType == DragType.WorldAxisMove || dragType == DragType.SelfPlaneMove || dragType == DragType.WorldPlaneMove); } }

        private void Awake()
        {
            m_inputEventer = GetComponent<InputEventer>();

            if (m_inputEventer)
            {
                m_inputEventer.onInitDragPlus.AddListener(OnInitializePotentialDrag);
                m_inputEventer.onDragPlus.AddListener(OnDrag);
                m_inputEventer.onEndDrag.AddListener(OnEndDrag);
            }

        }

        private void OnInitializePotentialDrag(GameObject _go, PointerEventData _eventData)
        {
            m_dragTarget = _go.transform;

            if (useRigibody)
            {
                curRigidbody.isKinematic = true;
            }

            if (!_eventData.pressEventCamera.orthographic)
            {
                float l_tan = Mathf.Tan(Mathf.Deg2Rad * _eventData.pressEventCamera.fieldOfView * 0.5f);

                float l_camAngle = Vector3.Angle(_eventData.pressEventCamera.transform.forward, _eventData.pointerPressRaycast.worldPosition - _eventData.pressEventCamera.transform.position);

                float l_cos = Mathf.Cos(Mathf.Deg2Rad * l_camAngle);

                m_dragRatio = (l_tan * (_eventData.pointerPressRaycast.distance + _eventData.pressEventCamera.nearClipPlane / l_cos) * l_cos) / (Screen.height * 0.5f);
            }
            else
            {
                m_dragRatio = _eventData.pressEventCamera.orthographicSize / (Screen.height * 0.5f);
            }

            if (dragType == DragType.WorldPlaneMove || dragType == DragType.SelfPlaneMove)
            {
                m_offsetWorldPos = Vector3.zero;

                Vector3 l_planeNormal = (dragType == DragType.WorldPlaneMove ? dragPlaneNormal : m_dragTarget.TransformDirection(dragPlaneNormal)).normalized;

                m_dragPlane = new Plane(l_planeNormal, _eventData.pointerPressRaycast.worldPosition);

                m_offsetWorldPos = _eventData.pointerPressRaycast.worldPosition - m_dragTarget.position;
            }
            else if (dragType == DragType.SelfAxisRotate)
            {
                if (dynamicRotateDir)
                    m_dragRotLocalPos = transform.InverseTransformPoint(_eventData.pointerPressRaycast.worldPosition);

                Vector3 l_tangent = Vector3.Cross(transform.TransformDirection(dragAxis.normalized), (_eventData.pointerPressRaycast.worldPosition - transform.position).normalized);
                Vector3 l_tangentStartScreenPos = _eventData.pressEventCamera.WorldToScreenPoint(_eventData.pointerPressRaycast.worldPosition);
                Vector3 l_tangentEndScreenPos = _eventData.pressEventCamera.WorldToScreenPoint(_eventData.pointerPressRaycast.worldPosition + l_tangent);
                Vector2 l_screneDir = (l_tangentEndScreenPos - l_tangentStartScreenPos).normalized;

                m_dragRotDir = l_screneDir;
            }
            else if (dragType == DragType.WorldAxisRotata)
            {
                if (dynamicRotateDir)
                    m_dragRotLocalPos = transform.InverseTransformPoint(_eventData.pointerPressRaycast.worldPosition);

                Vector3 l_tangent = Vector3.Cross(dragAxis.normalized, (_eventData.pointerPressRaycast.worldPosition - transform.position).normalized);
                Vector3 l_tangentStartScreenPos = _eventData.pressEventCamera.WorldToScreenPoint(_eventData.pointerPressRaycast.worldPosition);
                Vector3 l_tangentEndScreenPos = _eventData.pressEventCamera.WorldToScreenPoint(_eventData.pointerPressRaycast.worldPosition + l_tangent);
                Vector2 l_screneDir = (l_tangentEndScreenPos - l_tangentStartScreenPos).normalized;

                m_dragRotDir = l_screneDir;
            }
            else if (dragType == DragType.WorldAxisRotateOnPlane || dragType == DragType.SelfAxisRotateOnPlane)
            {
                Vector3 l_planeNormal = (dragType == DragType.WorldPlaneMove ? dragAxis : m_dragTarget.TransformDirection(dragAxis)).normalized;

                m_dragPlane = new Plane(l_planeNormal, _eventData.pointerPressRaycast.worldPosition);
            }

            m_dragPos = m_dragTarget.position;

            if (onStartDrag != null)
                onStartDrag.Invoke(m_dragTarget);
        }

        private void OnDrag(GameObject _go, PointerEventData _eventData)
        {
            if (isMove)
            {
                m_dragPos += GetTranslateValue(_eventData) * dragSpeed;

                if (!useRigibody)
                {
                    m_dragTarget.position = m_dragPos;
                }
                else
                {
                    curRigidbody.MovePosition(m_dragPos);
                }
            }
            else
            {
                switch (dragType)
                {
                    case DragType.WorldAxisRotata:
                        m_dragTarget.Rotate(dragAxis.normalized, GetRotateValue(_eventData) * (180f / Screen.height) * dragSpeed, Space.World);
                        break;
                    case DragType.SelfAxisRotate:
                        m_dragTarget.Rotate(dragAxis.normalized, GetRotateValue(_eventData) * (180f / Screen.height) * dragSpeed, Space.Self);
                        break;
                    case DragType.WorldAxisRotateOnPlane:

                        break;
                    case DragType.SelfAxisRotateOnPlane:

                        break;
                    default:

                        break;
                }
            }

            if (m_dragTarget)
                onDrag.Invoke(m_dragTarget);
        }

        private void OnEndDrag()
        {
            if (useRigibody)
            {
                curRigidbody.isKinematic = false;
            }

            if (onEndDrag != null)
                onEndDrag.Invoke(m_dragTarget);
        }

        private Vector3 GetTranslateValue(PointerEventData _eventData)
        {
            Vector3 l_translateValue = Vector3.zero;

            if (dragType == DragType.FreedomMove)
            {
                Vector3 moveValue = _eventData.delta * m_dragRatio;

                l_translateValue = _eventData.pressEventCamera.transform.TransformVector(moveValue);
            }
            else if (dragType == DragType.WorldAxisMove)
            {
                Vector2 l_worldAxisScreenDir = (_eventData.pressEventCamera.WorldToScreenPoint(m_dragTarget.position + dragAxis * _eventData.pointerPressRaycast.distance * 0.1f) - _eventData.pressEventCamera.WorldToScreenPoint(m_dragTarget.position)).normalized;

                if (l_worldAxisScreenDir != Vector2.zero)
                {
                    Vector3 l_moveProject = Vector3.Project(_eventData.delta, l_worldAxisScreenDir);

                    float moveRatio = l_moveProject.magnitude / l_worldAxisScreenDir.magnitude;

                    if (Vector2.Dot(_eventData.delta, l_worldAxisScreenDir) < 0f)
                    {
                        moveRatio = -moveRatio;
                    }

                    l_translateValue = moveRatio * m_dragRatio * (dragAxis.normalized);
                }
            }
            else if (dragType == DragType.SelfAxisMove)
            {
                Vector3 l_dragAxisWorld = m_dragTarget.TransformDirection(dragAxis.normalized);

                Vector3 l_localAxisScreenDir = (_eventData.pressEventCamera.WorldToScreenPoint(m_dragTarget.position + l_dragAxisWorld * _eventData.pointerPressRaycast.distance * 0.1f) - _eventData.pressEventCamera.WorldToScreenPoint(m_dragTarget.position)).normalized;

                if (l_localAxisScreenDir != Vector3.zero)
                {
                    Vector3 l_moveProject = Vector3.Project(_eventData.delta, l_localAxisScreenDir);

                    float moveRatio = l_moveProject.magnitude / l_localAxisScreenDir.magnitude;

                    if (Vector2.Dot(_eventData.delta, l_localAxisScreenDir) < 0f)
                    {
                        moveRatio = -moveRatio;
                    }

                    l_translateValue = moveRatio * m_dragRatio * (l_dragAxisWorld.normalized);
                }
            }
            else if (dragType == DragType.WorldPlaneMove || dragType == DragType.SelfPlaneMove)
            {
                Ray l_ray = _eventData.pressEventCamera.ScreenPointToRay(_eventData.position);

                float l_hitDistance = 0;

                if (m_dragPlane.Raycast(l_ray, out l_hitDistance))
                {
                    Vector3 l_hitPoint = l_ray.GetPoint(l_hitDistance);

                    l_translateValue = l_hitPoint - m_offsetWorldPos - m_dragTarget.position;
                }
            }

            return l_translateValue;
        }

        private float GetRotateValue(PointerEventData eventData)
        {
            if (dynamicRotateDir)
            {
                if (dragType == DragType.SelfAxisRotate)
                {
                    Vector3 l_worldPos = transform.TransformPoint(m_dragRotLocalPos);

                    Vector3 l_tangent = Vector3.Cross(transform.TransformDirection(dragAxis.normalized), (l_worldPos - transform.position).normalized);
                    Vector3 l_tangentStartScreenPos = eventData.pressEventCamera.WorldToScreenPoint(eventData.pointerPressRaycast.worldPosition);
                    Vector3 l_tangentEndScreenPos = eventData.pressEventCamera.WorldToScreenPoint(eventData.pointerPressRaycast.worldPosition + l_tangent);
                    Vector2 l_screneDir = (l_tangentEndScreenPos - l_tangentStartScreenPos).normalized;

                    m_dragRotDir = l_screneDir;
                }
                else if (dragType == DragType.WorldAxisRotata)
                {
                    Vector3 l_worldPos = transform.TransformPoint(m_dragRotLocalPos);

                    Vector3 l_tangent = Vector3.Cross(dragAxis.normalized, (l_worldPos - transform.position).normalized);
                    Vector3 l_tangentStartScreenPos = eventData.pressEventCamera.WorldToScreenPoint(eventData.pointerPressRaycast.worldPosition);
                    Vector3 l_tangentEndScreenPos = eventData.pressEventCamera.WorldToScreenPoint(eventData.pointerPressRaycast.worldPosition + l_tangent);
                    Vector2 l_screneDir = (l_tangentEndScreenPos - l_tangentStartScreenPos).normalized;

                    m_dragRotDir = l_screneDir;
                }
            }

            float l_rotValue = 0f;

            if (dragType == DragType.SelfAxisRotate || dragType == DragType.WorldAxisRotata)
            {
                float l_moveValue = Vector3.Project(eventData.delta, m_dragRotDir).magnitude;

                if (Vector2.Dot(eventData.delta, m_dragRotDir) < 0f)
                    l_moveValue = -l_moveValue;

                l_rotValue = l_moveValue;
            }
            else if (dragType == DragType.SelfAxisRotateOnPlane)
            {

            }

            return l_rotValue;
        }

        public enum DragType
        {
            None,

            FreedomMove,
            WorldAxisMove,
            SelfAxisMove,
            WorldPlaneMove,
            SelfPlaneMove,

            FreedomRotate,
            WorldAxisRotata,
            SelfAxisRotate,
            WorldAxisRotateOnPlane,
            SelfAxisRotateOnPlane,
        }
    }
}