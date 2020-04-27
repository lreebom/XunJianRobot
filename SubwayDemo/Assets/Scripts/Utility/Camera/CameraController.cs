using UnityEngine;
using System.Collections.Generic;

namespace TRDev.SafetyClient
{
    public class CameraController : MonoBehaviour
    {
        #region Privete Fields

        [SerializeField]
        private string m_cameraControlerName = "Main";

        [SerializeField]
        private Lreebom.UpdateType m_updateType = Lreebom.UpdateType.Update;

        [SerializeField]
        private bool enableHRotate = true;

        [SerializeField]
        private bool enableVRotate = true;

        /// <summary>
        /// 开启旋转功能
        /// </summary>
        [SerializeField]
        private bool m_enableRotate = true;

        /// <summary>
        /// 开启放大缩小功能
        /// </summary>
        [SerializeField]
        private bool m_enableZoom = true;

        /// <summary>
        /// 开启平移功能
        /// </summary>
        [SerializeField]
        private bool m_enableMove = true;

        /// <summary>
        /// 最小距离
        /// </summary>
        [SerializeField]
        private float m_minDistance = 1f;

        /// <summary>
        /// 最大距离
        /// </summary>
        [SerializeField]
        private float m_maxDistance = 50f;

        /// <summary>
        /// 最小正交Size
        /// </summary>
        [SerializeField]
        private float m_minOrthSize = 1f;

        /// <summary>
        /// 最大正交Size
        /// </summary>
        [SerializeField]
        private float m_maxOrthSize = 50f;

        /// <summary>
        /// 开启碰撞规避
        /// </summary>
        [SerializeField]
        private bool m_enableAvoidCollider = false;

        private bool m_isAvoidingCollider = false;

        /// <summary>
        /// 碰撞规避的层
        /// </summary>
        [SerializeField]
        private LayerMask m_avoidColliderMask = 1;

        private CharacterController m_charaCtrl;

        private Camera[] m_childCameras = null;

        private AvoidUI m_avoidUI = null;


        [SerializeField]
        private bool m_controlChildCameraTransform = true;

        #region Mouse Control

        /// <summary>
        /// 鼠标 推拉 时间
        /// </summary>
        private float m_mouseZoomTweenTime = 0.5f;
        #endregion

        #region Touch Control

        private float m_touch_MoveDelayTime = 0.2f;
        #endregion

        protected Camera m_camera;

        private Transform m_centerTsfm;
        private Transform m_distanceTsfm;
        private Transform m_cameraTsfm;

        private bool m_mouseMainDownIsHitUI = true;
        private bool m_mouseOtherDownIsHitUI = true;

        private float m_mouseZoomSpeed = 0f;
        private float m_mouseZoomStartSpeed = 0f;
        private float m_mouseZoomTime = float.MaxValue;

        private Vector3 m_lastMousePos = Vector3.zero;

        private bool m_touchBeganIsHitUI = true;
        private float m_touchMoveDelayTime = 0f;
        private int m_lastTouchCount = 0;

        private Vector3 m_lastTouchPos = Vector3.zero;
        private Vector3 m_lastTouchPos0 = Vector3.zero;
        private Vector3 m_lastTouchPos1 = Vector3.zero;

        private bool m_isDraging = false;

        private bool m_enableControl = true;

        public class OnTransformStateChangeEvent : UnityEngine.Events.UnityEvent<Transform> { }
        public OnTransformStateChangeEvent onTransformStateChange = new OnTransformStateChangeEvent();

        private float currentDPI
        {
            get
            {
                if (Screen.dpi == 0f)
                {
                    return 96f;
                }
                else
                {
                    return Screen.dpi;
                }
            }
        }

        #endregion

        private static Dictionary<string, CameraController> m_cameraControlDic = new Dictionary<string, CameraController>();

        #region Public Attribute
        public Camera myCamera
        {
            get
            {
                if (m_camera == null)
                    m_camera = gameObject.GetComponent<Camera>();
                return m_camera;
            }
        }

        public bool isDraging
        {
            get { return m_isDraging; }
        }

        [System.Obsolete("is old, use centerTransform!")]
        public Transform cameraParent
        {
            get { return centerTransform; }
        }

        public bool isOrtho
        {
            get
            {
                return myCamera.orthographic;
            }
            set
            {
                if (myCamera.orthographic != value)
                {
                    myCamera.orthographic = value;

                    if (myCamera.orthographic)
                    {
                        float targetOrthoSize = Mathf.Sin(myCamera.fieldOfView * 0.5f * Mathf.Deg2Rad) * distance;
                        myCamera.orthographicSize = targetOrthoSize;
                    }
                    else
                    {
                        float targetDistance = myCamera.orthographicSize / Mathf.Sin(myCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                        distance = targetDistance;
                    }

                    UpdateChildCameras();
                }
            }
        }

        public float distance
        {
            get
            {
                return -distanceTransform.localPosition.z;
            }
            set
            {
                CancleMousePushPull();

                Vector3 localPos = new Vector3(0f, 0f, -value);

                distanceTransform.localPosition = localPos;
            }
        }

        public bool enableRotate
        {
            get { return m_enableRotate; }
            set { m_enableRotate = value; }
        }
        public bool enableZoom
        {
            get { return m_enableZoom; }
            set { m_enableZoom = value; }
        }
        public bool enableMove
        {
            get { return m_enableMove; }
            set { m_enableMove = value; }
        }

        public float minDistance
        {
            get
            {
                return m_minDistance;
            }
            set
            {
                m_minDistance = value;
                if (m_enableAvoidCollider)
                {
                    charaCtrl.radius = minDistance;
                    charaCtrl.height = minDistance * 2f;
                }
            }
        }
        public float maxDistance
        {
            get
            {
                return m_maxDistance;
            }
            set
            {
                m_maxDistance = value;
            }
        }
        public float minOrthSize
        {
            get
            {
                return m_minOrthSize;
            }
            set
            {
                m_minOrthSize = value;
            }
        }
        public float maxOrthSize
        {
            get
            {
                return m_maxOrthSize;
            }
            set
            {
                m_maxOrthSize = value;
            }
        }

        public bool isZooming { get; set; }

        public Transform cameraTransform
        {
            get
            {
                if (m_cameraTsfm == null)
                {
                    m_cameraTsfm = transform;
                }
                return m_cameraTsfm;
            }
        }

        public Transform distanceTransform
        {
            get
            {
                if (m_distanceTsfm == null)
                {
                    m_distanceTsfm = cameraTransform.parent;
                }
                return m_distanceTsfm;
            }
        }

        public Transform centerTransform
        {
            get
            {
                if (m_centerTsfm == null)
                {
                    m_centerTsfm = cameraTransform.parent.parent;
                }
                return m_centerTsfm;
            }
        }

        public bool enableAvoidCollider
        {
            get { return m_enableAvoidCollider; }
            set
            {
                if (m_enableAvoidCollider != value)
                {
                    m_enableAvoidCollider = value;
                    if (m_enableAvoidCollider)
                    {
                        charaCtrl.enabled = true;
                    }
                    else
                    {
                        if (m_charaCtrl)
                            m_charaCtrl.enabled = false;
                    }
                }
            }
        }

        public CharacterController charaCtrl
        {
            get
            {
                if (m_charaCtrl == null)
                {
                    m_charaCtrl = centerTransform.gameObject.GetComponent<CharacterController>();
                    if (m_charaCtrl == null)
                        m_charaCtrl = centerTransform.gameObject.AddComponent<CharacterController>();

                    m_charaCtrl.slopeLimit = 180f;
                    m_charaCtrl.radius = minDistance;
                    m_charaCtrl.height = minDistance * 2f;
                }
                return m_charaCtrl;
            }
        }

        public bool enableControl
        {
            get
            {
                return m_enableControl;
            }
            set
            {
                m_enableControl = value;

                m_mouseMainDownIsHitUI = true;
                m_mouseOtherDownIsHitUI = true;
                m_touchBeganIsHitUI = true;
            }
        }

        #endregion
        protected virtual void Awake()
        {
            if (!m_cameraControlDic.ContainsKey(m_cameraControlerName))
            {
                m_cameraControlDic.Add(m_cameraControlerName, this);

                enableControl = true;
            }
            else
            {
                Debug.LogError("此场景中有重名的 CameraController");
            }
        }

        protected virtual void Start()
        {
            cameraTransform.localPosition = new Vector3(0f, 0f, cameraTransform.localPosition.z);
            distanceTransform.localPosition = new Vector3(0f, 0f, distanceTransform.localPosition.z);

            if (transform.childCount > 0)
            {
                m_childCameras = transform.GetComponentsInChildren<Camera>();
            }

            m_avoidUI = gameObject.GetComponent<AvoidUI>();

            m_touchMoveDelayTime = m_touch_MoveDelayTime;

            //            #region Rotate Ratio
            //#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
            //            rotateRatio = (96f / CoolPlayUtility.CurrentDPI) * 0.2f;
            //#else
            //            rotateRatio = (96f / CoolPlayUtility.CurrentDPI) * 0.8f;
            //#endif
            //            #endregion

            //            touchZoomRatio = (96f / CoolPlayUtility.CurrentDPI) * 0.006f;
        }

        protected virtual void OnEnable()
        {
            m_mouseMainDownIsHitUI = true;
            CancleMousePushPull();
        }

        protected virtual void Update()
        {
            if (m_updateType == Lreebom.UpdateType.Update)
            {
                ControlUpdate();
            }
        }

        protected virtual void LateUpdate()
        {
            if (m_updateType == Lreebom.UpdateType.LateUpdate)
            {
                ControlUpdate();
            }
        }

        protected virtual void OnDestroy()
        {
            if (m_cameraControlDic.ContainsKey(m_cameraControlerName))
            {
                m_cameraControlDic.Remove(m_cameraControlerName);
            }
        }

        #region Private Method

        protected void ControlUpdate()
        {
            if (enableControl)
            {
                TouchUpdateEvent();

                if (Input.touchCount <= 0)
                    MouseUpdateEvent();
            }

            AvoidCollider();
        }

        protected void MouseUpdateEvent()
        {
            bool curIsDraging = false;

            bool l_mainIsHitUI = true;
            bool l_otherIsHitUI = true;

            if (m_avoidUI)
            {
                m_avoidUI.IsHitUIBy2Layer(Input.mousePosition, out l_mainIsHitUI, out l_otherIsHitUI);
            }
            else
            {
                l_mainIsHitUI = false;
                l_otherIsHitUI = false;
            }

            if (Input.GetMouseButtonDown(0))
            {
                m_mouseMainDownIsHitUI = l_mainIsHitUI;
                m_lastMousePos = Input.mousePosition;
            }
            if (Input.GetMouseButtonDown(1))
            {
                m_mouseOtherDownIsHitUI = l_otherIsHitUI;
                m_lastMousePos = Input.mousePosition;
            }

            Vector3 l_moveVec = Input.mousePosition - m_lastMousePos;

            if (!m_mouseMainDownIsHitUI)
            {
                if (m_enableRotate && Input.GetMouseButton(0))
                {
                    curIsDraging = true;

                    if (l_moveVec != Vector3.zero)
                        RotateCamera(l_moveVec);
                }
            }

            if (!m_mouseOtherDownIsHitUI)
            {
                if (m_enableMove && Input.GetMouseButton(1))
                {
                    curIsDraging = true;

                    if (l_moveVec != Vector3.zero)
                        MouseMoveCamera(l_moveVec);
                }
            }

            if (m_enableZoom)
            {
                MousePushPullCamera(l_otherIsHitUI);
            }

            m_lastMousePos = Input.mousePosition;

            m_isDraging = curIsDraging;
        }

        protected void TouchUpdateEvent()
        {
            bool l_curIsDraging = false;

            var l_curTouchCount = Input.touchCount;

            if (l_curTouchCount > 0)
            {
                for (int i = 0; i < l_curTouchCount; i++)
                {
                    Touch l_curTouch = Input.GetTouch(i);

                    if (l_curTouch.phase == TouchPhase.Began)
                    {
                        switch (i)
                        {
                            case 0:
                                m_lastTouchPos0 = m_lastTouchPos = l_curTouch.position;
                                m_touchBeganIsHitUI = m_avoidUI.IsHitUI(l_curTouch.position);
                                break;
                            case 1:
                                m_lastTouchPos1 = l_curTouch.position;
                                break;
                        }
                    }
                }
            }

            if (m_lastTouchCount != l_curTouchCount)
            {
                if (l_curTouchCount == 1)
                {
                    m_lastTouchPos = Input.GetTouch(0).position;

                }
                else if (l_curTouchCount > 1)
                {
                    m_lastTouchPos0 = Input.GetTouch(0).position;
                    m_lastTouchPos1 = Input.GetTouch(1).position;
                }

                m_lastTouchCount = l_curTouchCount;

                return;
            }

            if (!m_touchBeganIsHitUI && l_curTouchCount > 0)
            {
                if (l_curTouchCount == 1)
                {
                    Touch l_curTouch = Input.GetTouch(0);

                    if (m_enableRotate)
                    {
                        if (m_touchMoveDelayTime >= m_touch_MoveDelayTime)
                        {
                            Vector3 curTouchPos = l_curTouch.position;

                            if (l_curTouch.phase == TouchPhase.Moved && m_lastTouchPos != curTouchPos)
                            {
                                RotateCamera(curTouchPos - m_lastTouchPos);
                            }
                        }
                    }

                    m_lastTouchPos = l_curTouch.position;
                }
                else if (l_curTouchCount >= 2)
                {
                    m_touchMoveDelayTime = 0f;
                    TouchMoveZoomCamera();
                }

                l_curIsDraging = true;
            }

            if (m_touchMoveDelayTime < m_touch_MoveDelayTime)
                m_touchMoveDelayTime += Time.unscaledDeltaTime;

            m_isDraging = l_curIsDraging;
        }

        protected void MousePushPullCamera(bool _hitUI)
        {
            if (!_hitUI)
            {
                float l_mouseScrollWheel = Input.GetAxis("Mouse ScrollWheel");

                if (l_mouseScrollWheel != 0f)
                {
                    m_mouseZoomStartSpeed = m_mouseZoomSpeed + l_mouseScrollWheel;
                    m_mouseZoomTime = 0f;
                }
            }

            if (m_mouseZoomTime < m_mouseZoomTweenTime)
            {
                isZooming = true;

                m_mouseZoomTime += Time.unscaledDeltaTime;

                float l_t = Mathf.Clamp01(m_mouseZoomTime / m_mouseZoomTweenTime);

                m_mouseZoomSpeed = Mathf.Lerp(m_mouseZoomStartSpeed, 0f, l_t);

                float l_speedZoom = 3f;
#if UNITY_5 && UNITY_WEBGL && !UNITY_EDITOR
                l_speedZoom = 0.6f;
#endif

                if (myCamera.orthographic)
                {
                    float l_curOrthoSize = myCamera.orthographicSize * (1 - m_mouseZoomSpeed * Time.unscaledDeltaTime * l_speedZoom);

                    l_curOrthoSize = Mathf.Clamp(l_curOrthoSize, m_minOrthSize, m_maxOrthSize);

                    myCamera.orthographicSize = l_curOrthoSize;
                }
                else
                {
                    bool l_shouldZoom = true;

                    float l_zoomRatio = (1f - m_mouseZoomSpeed * Time.unscaledDeltaTime * l_speedZoom);

                    if (l_zoomRatio < 1f)
                    {
                        if (cameraTransform.parent == distanceTransform)
                        {
                            distanceTransform.localPosition = new Vector3(0f, 0f, -Mathf.Clamp(distance - cameraTransform.localPosition.z, minDistance, maxDistance));
                            cameraTransform.localPosition = Vector3.zero;
                        }
                    }
                    else
                    {
                        if (m_isAvoidingCollider && l_zoomRatio > 1f)
                        {
                            l_shouldZoom = false;
                        }
                    }

                    if (l_shouldZoom)
                    {
                        float l_Distance = Mathf.Clamp(distance * l_zoomRatio, m_minDistance, m_maxDistance);

                        distanceTransform.localPosition = new Vector3(0f, 0f, -l_Distance);

                        onTransformStateChange.Invoke(transform);
                    }
                }

                UpdateChildCameras();
            }
            else
            {
                isZooming = false;
            }
        }

        protected virtual void RotateCamera(Vector3 _moveVec)
        {
            float l_rotateRatio = 180f / Screen.height;
            //float l_rotateRatio = (Screen.width + Screen.height) / (currentDPI * 54f);
            if (enableHRotate)
            {
                centerTransform.Rotate(Vector3.up, _moveVec.x * l_rotateRatio, Space.World);
            }
            if (enableVRotate)
            {
                centerTransform.Rotate(Vector3.left, _moveVec.y * l_rotateRatio, Space.Self);
            }

            onTransformStateChange.Invoke(transform);
        }

        protected void MouseMoveCamera(Vector3 _moveVec)
        {
            float l_heightRatio;
            float l_widthRatio;

            if (!myCamera.orthographic)
            {
                float l_planeDistance = Mathf.Abs(distanceTransform.localPosition.z);
                float l_tan = Mathf.Tan(Mathf.Deg2Rad * myCamera.fieldOfView / 2f);
                l_heightRatio = (Screen.height / 2f) / (l_tan * l_planeDistance);
                l_widthRatio = (Screen.width / 2f) / (l_tan * ((float)Screen.width / (float)Screen.height) * l_planeDistance);
            }
            else
            {
                l_heightRatio = Screen.height * 0.5f / myCamera.orthographicSize;
                l_widthRatio = Screen.width * 0.5f / (myCamera.orthographicSize * ((float)Screen.width / (float)Screen.height));
            }

            Vector3 l_moveVector = -_moveVec;
            l_moveVector.x /= l_widthRatio;
            l_moveVector.y /= l_heightRatio;
            l_moveVector.z = 0f;

            if (m_enableAvoidCollider)
            {
                Vector3 l_moveV = cameraTransform.TransformVector(l_moveVector);

                charaCtrl.Move(l_moveV);
            }
            else
            {
                centerTransform.Translate(l_moveVector, cameraTransform);
            }

            onTransformStateChange.Invoke(transform);
        }

        protected void TouchMoveZoomCamera()
        {
            Touch l_touch0 = Input.GetTouch(0);
            Touch l_touch1 = Input.GetTouch(1);

            if (l_touch0.phase != TouchPhase.Moved && l_touch0.phase != TouchPhase.Stationary)
            {
                m_lastTouchPos0 = l_touch0.position;
                return;
            }
            if (l_touch1.phase != TouchPhase.Moved && l_touch1.phase != TouchPhase.Stationary)
            {
                m_lastTouchPos1 = l_touch1.position;
                return;
            }

            if (l_touch0.phase == TouchPhase.Moved || l_touch1.phase == TouchPhase.Moved)
            {
                Vector3 l_curTouchPos0 = l_touch0.position;
                Vector3 l_curTouchPos1 = l_touch1.position;

                if (m_enableZoom)
                {
                    #region Touch Zoom
                    float l_touchZoomRatio = 1f / Screen.height;

                    float l_curTwoFingersDistance = Vector2.Distance(l_curTouchPos0, l_curTouchPos1);

                    float l_lastTwoFingersDistance = Vector2.Distance(m_lastTouchPos0, m_lastTouchPos1);

                    float l_pushPullValue = (l_lastTwoFingersDistance - l_curTwoFingersDistance);

                    if (myCamera.orthographic)
                    {
                        float l_targetOrthSize = myCamera.orthographicSize + l_pushPullValue * myCamera.orthographicSize * l_touchZoomRatio;

                        l_targetOrthSize = Mathf.Clamp(l_targetOrthSize, m_minOrthSize, m_maxOrthSize);

                        myCamera.orthographicSize = l_targetOrthSize;
                    }
                    else
                    {
                        bool l_shouldZoom = true;

                        if (l_pushPullValue > 0f)
                        {
                            if (cameraTransform.parent == distanceTransform)
                            {
                                distanceTransform.localPosition = new Vector3(0f, 0f, -Mathf.Clamp(distance - cameraTransform.localPosition.z, minDistance, maxDistance));
                                cameraTransform.localPosition = Vector3.zero;
                            }
                        }
                        else
                        {
                            if (m_isAvoidingCollider && l_pushPullValue < 0f)
                            {
                                l_shouldZoom = false;
                            }
                        }

                        if (l_shouldZoom)
                        {
                            float l_Distance = distance + l_pushPullValue * distance * l_touchZoomRatio;

                            l_Distance = Mathf.Clamp(l_Distance, minDistance, maxDistance);

                            distanceTransform.localPosition = new Vector3(0f, 0f, -l_Distance);

                            onTransformStateChange.Invoke(transform);
                        }
                    }

                    UpdateChildCameras();
                    #endregion
                }

                if (m_enableMove)
                {
                    #region Touch Move
                    Vector3 l_moveVector = Vector3.zero;

                    l_moveVector -= (l_curTouchPos0 - m_lastTouchPos0);
                    l_moveVector -= (l_curTouchPos1 - m_lastTouchPos1);

                    l_moveVector /= 2;
                    l_moveVector.z = 0f;

                    if (l_moveVector != Vector3.zero)
                    {
                        float l_heightRatio;
                        float l_widthRatio;

                        if (!myCamera.orthographic)
                        {
                            float l_planeDistance = Mathf.Abs(distanceTransform.localPosition.z);
                            float l_tan = Mathf.Tan(Mathf.Deg2Rad * myCamera.fieldOfView / 2f);
                            l_heightRatio = (Screen.height / 2f) / (l_tan * l_planeDistance);
                            l_widthRatio = (Screen.width / 2f) / (l_tan * ((float)Screen.width / (float)Screen.height) * l_planeDistance);
                        }
                        else
                        {
                            l_heightRatio = Screen.height * 0.5f / myCamera.orthographicSize;
                            l_widthRatio = Screen.width * 0.5f / (myCamera.orthographicSize * ((float)Screen.width / (float)Screen.height));
                        }

                        l_moveVector.x /= l_widthRatio;
                        l_moveVector.y /= l_heightRatio;

                        if (m_enableAvoidCollider)
                        {
                            Vector3 l_moveV = cameraTransform.TransformVector(l_moveVector);

                            charaCtrl.Move(l_moveV);
                        }
                        else
                        {
                            centerTransform.Translate(l_moveVector, cameraTransform);
                        }

                        onTransformStateChange.Invoke(transform);
                    }
                    #endregion
                }

                m_lastTouchPos0 = l_curTouchPos0;
                m_lastTouchPos1 = l_curTouchPos1;
            }
            else
            {
                m_lastTouchPos0 = l_touch0.position;
                m_lastTouchPos1 = l_touch1.position;
            }
        }

        void AvoidCollider()
        {
            if (isOrtho) return;

            if (m_enableAvoidCollider)
            {
                Ray l_ray = new Ray(centerTransform.position, -centerTransform.forward);

                RaycastHit l_hit;

                float l_dis = 0f;

                float l_inSideLength = 0f;

                if (Physics.Raycast(l_ray, out l_hit, distance + myCamera.nearClipPlane * 3f, m_avoidColliderMask))
                {
                    l_inSideLength = myCamera.nearClipPlane * 3f * (Mathf.Sin(Vector3.Angle(l_ray.direction, l_hit.normal) * Mathf.Deg2Rad));

                    l_dis = l_hit.distance - l_inSideLength;

                    if (l_dis <= distance)
                    {
                        m_isAvoidingCollider = true;
                    }
                    else
                    {
                        m_isAvoidingCollider = false;
                    }
                }
                else
                {
                    m_isAvoidingCollider = false;
                }

                if (m_isAvoidingCollider)
                {
                    float l_curDis = distance - l_dis + l_inSideLength;

                    //float l_maxDis = distance - minDistance;

                    //float l_outDis = l_curDis - l_maxDis;

                    Vector3 l_targetLocalPos = new Vector3(0f, 0f, l_curDis);

                    if (!Mathf.Approximately(cameraTransform.localPosition.z, l_targetLocalPos.z))
                        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, l_targetLocalPos, Time.unscaledDeltaTime * 16f);
                }
                else
                {
                    if (!Mathf.Approximately(cameraTransform.localPosition.z, 0f))
                        cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, Vector3.zero, Time.unscaledDeltaTime * 8f);
                }
            }
        }

        #endregion

        public void UpdateChildCameras()
        {
            if (m_childCameras != null && m_childCameras.Length > 0)
            {
                bool isOrtho = myCamera.orthographic;

                foreach (Camera one in m_childCameras)
                {
                    if (one != myCamera)
                    {
                        if (isOrtho)
                        {
                            if (!one.orthographic)
                            {
                                one.orthographic = true;
                            }
                            one.orthographicSize = myCamera.orthographicSize;
                        }
                        else if (m_controlChildCameraTransform)
                        {
                            if (one.orthographic)
                            {
                                one.orthographic = false;
                            }
                            one.transform.localPosition = Vector3.zero;
                            one.transform.localRotation = Quaternion.identity;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 立即退出鼠标滚轮控制的推拉效果
        /// </summary>
        public void CancleMousePushPull()
        {
            m_mouseZoomTime = 1f;
        }

        public static CameraController GetByName(string _name)
        {
            if (m_cameraControlDic.ContainsKey(_name))
            {
                return m_cameraControlDic[_name];
            }
            else
            {
                return null;
            }
        }

        public static T GetByName<T>(string _name) where T : CameraController
        {
            T t = null;

            if (m_cameraControlDic.ContainsKey(_name))
            {
                t = m_cameraControlDic[_name].gameObject.GetComponent<T>();
            }

            return t;
        }

    }
}