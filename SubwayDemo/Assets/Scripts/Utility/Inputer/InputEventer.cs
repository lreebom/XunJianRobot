using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace TRDev.SafetyClient
{
    /// <summary>
    /// Version 1.4
    ///     1.4     添加 m_curHoveringDic清空
    ///     1.3     添加 avoidChildList isOnlySelfEvent
    ///     1.2     修复 DoubleClick 在触摸设备上的Bug
    ///     1.1     修复 DoubleClick 事件
    ///     1.0     初始版本
    /// </summary>
    public class InputEventer : MonoBehaviour,
        IPointerClickHandler,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerEnterHandler,
        IPointerExitHandler,

        IInitializePotentialDragHandler,
        IBeginDragHandler,
        IDragHandler,
        IEndDragHandler
    {
        /// <summary>
        /// 只接受自己的事件
        /// </summary>
        public bool isOnlySelfEvent = false;

        /// <summary>
        /// 忽略事件的子物体
        /// </summary>
        public List<GameObject> avoidChildList = new System.Collections.Generic.List<GameObject>();

        /// <summary>
        /// 屏蔽自己的事件
        /// </summary>
        public bool avoidSelfEvent = false;

        /// <summary>
        /// 把自己的事件转发给其他
        /// </summary>
        public List<InputEventer> notifyTargetList = new List<InputEventer>();

        /// <summary>
        /// 允许相应事件的Button
        /// </summary>
        public PointerEventData.InputButton arrowedInputButton = PointerEventData.InputButton.Left;

        public bool bubbleAllDragEvent = false;

        private static Dictionary<int, InputEventer> m_curHoveringDic = new Dictionary<int, InputEventer>();

        #region Hover Press Click
        private bool m_isHovering = false;

        private bool m_isPressing = false;

        //private float m_pressTime = 0f;
        //private float m_longPressTime = 1f;
        //private bool m_isInvokeLongPress = false;
        //private PointerEventData m_pressPointerEventData;

        private int m_lastClickID = -32;
        private float m_lastClickTime = -2f;
        private int m_dbClickCount = 0;

        private bool m_isHoveringNoPressed = false;
        #endregion

        #region Drag
        private bool m_isDraging = false;
        private bool m_dragIsBreaked = false;
        private PointerEventData m_dragEventData;
        #endregion

        /// <summary>
        /// isHovering
        /// </summary>
        public bool isHovering
        {
            get { return m_isHovering; }
        }

        /// <summary>
        /// isPressing
        /// </summary>
        public bool isPressing
        {
            get { return m_isPressing; }
        }

        #region Event Class
        public class OnClickEvent : UnityEvent { }
        public class OnDoubleClickEvent : UnityEvent { }
        public class OnPressEvent : UnityEvent<bool> { }
        public class OnHoverEvent : UnityEvent<bool> { }
        public class OnLongPressEvent : UnityEvent { }
        public class OnDragEvent : UnityEvent { }

        public class OnClickEventPlus : UnityEvent<GameObject, PointerEventData> { }
        public class OnDoubleClickEventPlus : UnityEvent<GameObject, PointerEventData> { }
        public class OnPressEventPlus : UnityEvent<GameObject, bool, PointerEventData> { }
        public class OnHoverEventPlus : UnityEvent<GameObject, bool, PointerEventData> { }
        public class OnLongPressEventPlus : UnityEvent<GameObject, PointerEventData> { }
        public class OnDragEventPlus : UnityEvent<GameObject, PointerEventData> { }


        public class OnPressNullEvent : UnityEvent<bool> { }

        #endregion

        #region Normal Event
        /// <summary>
        /// 点击事件
        /// </summary>
        public OnClickEvent onClick = new OnClickEvent();

        /// <summary>
        /// 没有拖拽的点击事件
        /// </summary>
        public OnClickEvent onClickNoDrag = new OnClickEvent();

        /// <summary>
        /// 双击事件
        /// </summary>
        public OnDoubleClickEvent onDoubleClick = new OnDoubleClickEvent();

        /// <summary>
        /// Press事件
        /// </summary>
        public OnPressEvent onPress = new OnPressEvent();

        /// <summary>
        /// Hover事件
        /// </summary>
        public OnHoverEvent onHover = new OnHoverEvent();

        /// <summary>
        /// 没有Press的Hover事件
        /// </summary>
        public OnHoverEvent onHoverNoPress = new OnHoverEvent();

        /// <summary>
        /// 长按事件(未完成)
        /// </summary>
        public OnLongPressEvent onLongPress = new OnLongPressEvent();

        /// <summary>
        /// 拖拽初始化事件
        /// </summary>
        public OnDragEvent onInitDrag = new OnDragEvent();

        /// <summary>
        /// 开始拖拽事件
        /// </summary>
        public OnDragEvent onBeginDrag = new OnDragEvent();

        /// <summary>
        /// 拖拽中事件
        /// </summary>
        public OnDragEvent onDrag = new OnDragEvent();

        /// <summary>
        /// 拖拽完成事件
        /// </summary>
        public OnDragEvent onEndDrag = new OnDragEvent();

        #endregion

        #region Plus Event
        /// <summary>
        /// 点击事件Plus
        /// </summary>
        public OnClickEventPlus onClickPlus = new OnClickEventPlus();

        /// <summary>
        /// 没有拖拽的点击事件Plus
        /// </summary>
        public OnClickEventPlus onClickNoDragPlus = new OnClickEventPlus();

        /// <summary>
        /// 双击事件Plus
        /// </summary>
        public OnDoubleClickEventPlus onDoubleClickPlus = new OnDoubleClickEventPlus();

        /// <summary>
        /// Press事件Plus
        /// </summary>
        public OnPressEventPlus onPressPlus = new OnPressEventPlus();

        /// <summary>
        /// Hover事件Plus
        /// </summary>
        public OnHoverEventPlus onHoverPlus = new OnHoverEventPlus();

        /// <summary>
        /// 没有Press的Hover事件Plus
        /// </summary>
        public OnHoverEventPlus onHoverNoPressPlus = new OnHoverEventPlus();

        /// <summary>
        /// 长按事件Plus(未完成)
        /// </summary>
        public OnLongPressEventPlus onLongPressPlus = new OnLongPressEventPlus();

        /// <summary>
        /// 拖拽初始化事件Plus
        /// </summary>
        public OnDragEventPlus onInitDragPlus = new OnDragEventPlus();

        /// <summary>
        /// 开始拖拽事件Plus
        /// </summary>
        public OnDragEventPlus onBeginDragPlus = new OnDragEventPlus();

        /// <summary>
        /// 拖拽中事件Plus
        /// </summary>
        public OnDragEventPlus onDragPlus = new OnDragEventPlus();

        /// <summary>
        /// 拖拽完成事件Plus
        /// </summary>
        public OnDragEventPlus onEndDragPlus = new OnDragEventPlus();
        #endregion

        //protected virtual void LateUpdate()
        //{
        //    if (m_isPressing && !m_isInvokeLongPress)
        //    {
        //        if (Vector2.Distance(Input.mousePosition, m_pressPointerEventData.position) > 4f)
        //        {
        //            //未完成
        //            m_isInvokeLongPress = true;
        //            return;
        //        }

        //        m_pressTime += Time.deltaTime;

        //        if (m_pressTime > m_longPressTime)
        //        {
        //            m_isInvokeLongPress = true;

        //            onLongPress.Invoke();

        //            onLongPressPlus.Invoke(gameObject, m_pressPointerEventData);
        //        }
        //    }
        //}

        //private void OnDestroy()
        //{
        //    foreach (int one in m_curHoveringDic.Keys)
        //    {
        //        if (m_curHoveringDic[one] == this)
        //        {
        //            m_curHoveringDic.Remove(one);
        //        }
        //    }
        //}

        protected bool IsEventButton(PointerEventData _eventData)
        {
            if (_eventData.pointerId < 0)
                return _eventData.button == arrowedInputButton;
            else
                return _eventData.pointerId == 0;
        }

        public virtual void OnPointerEnter(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                m_isHovering = true;

                if (!m_isPressing)
                    SetHoveringDic(_eventData.pointerId, true);

                UpdateHoveringNoPressed(_eventData);

                onHoverPlus.Invoke(gameObject, true, _eventData);
                onHover.Invoke(true);
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerEnter(_eventData);
                }
            }
        }

        public virtual void OnPointerExit(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                m_isHovering = false;

                if (!m_isPressing)
                    SetHoveringDic(_eventData.pointerId, false);

                UpdateHoveringNoPressed(_eventData);

                onHoverPlus.Invoke(gameObject, false, _eventData);
                onHover.Invoke(false);
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerExit(_eventData);
                }
            }
        }

        public virtual void OnPointerDown(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                UpdateHoveringNoPressed(_eventData);

                //m_pressTime = 0f;
                //m_isInvokeLongPress = false;
                m_isPressing = true;
                //m_pressPointerEventData = _eventData;

                onPressPlus.Invoke(gameObject, true, _eventData);
                onPress.Invoke(true);
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerDown(_eventData);
                }
            }
        }

        public virtual void OnPointerUp(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                m_isPressing = false;

                SetHoveringDic(_eventData.pointerId, m_isHovering);

                UpdateHoveringNoPressed(_eventData);

                onPressPlus.Invoke(gameObject, false, _eventData);
                onPress.Invoke(false);
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerUp(_eventData);
                }
            }
        }

        public virtual void OnPointerClick(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onClickPlus.Invoke(gameObject, _eventData);
                onClick.Invoke();

                if (!_eventData.dragging)
                {
                    onClickNoDragPlus.Invoke(gameObject, _eventData);
                    onClickNoDrag.Invoke();
                }

                if (m_lastClickID > -32)
                {
                    if (m_lastClickID == _eventData.pointerId)
                    {
                        if (_eventData.clickTime - m_lastClickTime < 0.3f)
                        {
                            m_dbClickCount++;

                            if (m_dbClickCount >= 2)
                            {
                                onDoubleClickPlus.Invoke(gameObject, _eventData);
                                onDoubleClick.Invoke();
                                m_dbClickCount = 0;
                            }
                            else
                            {
                                m_dbClickCount = 1;
                            }
                        }
                        else
                        {
                            m_dbClickCount = 1;
                        }
                    }
                    else
                    {
                        m_dbClickCount = 1;
                        m_lastClickID = _eventData.pointerId;
                    }

                    m_lastClickTime = _eventData.clickTime;
                }
                else
                {
                    m_dbClickCount = 1;
                    m_lastClickID = _eventData.pointerId;
                    m_lastClickTime = _eventData.clickTime;
                }
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerClick(_eventData);
                }
            }
        }

        public virtual void OnInitializePotentialDrag(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onInitDragPlus.Invoke(gameObject, _eventData);
                onInitDrag.Invoke();
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnInitializePotentialDrag(_eventData);
                }
            }

            if (bubbleAllDragEvent)
            {
                if (transform.parent != null)
                    ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.initializePotentialDrag);
            }
        }

        public virtual void OnBeginDrag(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            m_isDraging = true;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onBeginDragPlus.Invoke(gameObject, _eventData);
                onBeginDrag.Invoke();
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnBeginDrag(_eventData);
                }
            }

            if (bubbleAllDragEvent)
            {
                if (transform.parent != null)
                    ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.beginDragHandler);
            }
        }

        public virtual void OnDrag(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (m_dragIsBreaked) return;

            m_dragEventData = _eventData;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onDragPlus.Invoke(gameObject, _eventData);
                onDrag.Invoke();
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnDrag(_eventData);
                }
            }

            if (bubbleAllDragEvent)
            {
                if (transform.parent != null)
                    ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.dragHandler);
            }
        }

        public virtual void OnEndDrag(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (m_dragIsBreaked)
            {
                m_dragIsBreaked = false;
                return;
            }

            m_isDraging = false;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onEndDragPlus.Invoke(gameObject, _eventData);
                onEndDrag.Invoke();
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnEndDrag(_eventData);
                }
            }

            if (bubbleAllDragEvent)
            {
                if (transform.parent != null)
                    ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.endDragHandler);
            }
        }

        private void OnPointerEnterNoPress(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onHoverNoPressPlus.Invoke(gameObject, true, _eventData);
                onHoverNoPress.Invoke(true);
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerEnterNoPress(_eventData);
                }
            }
        }

        private void OnPointerExitNoPress(PointerEventData _eventData)
        {
            if (!IsEventButton(_eventData)) return;

            if (!avoidSelfEvent && (isOnlySelfEvent ? (_eventData.rawPointerPress == gameObject) : (!avoidChildList.Contains(_eventData.rawPointerPress))))
            {
                onHoverNoPressPlus.Invoke(gameObject, false, _eventData);
                onHoverNoPress.Invoke(false);
            }

            if (notifyTargetList.Count > 0)
            {
                foreach (InputEventer one in notifyTargetList)
                {
                    if (one != null)
                        one.OnPointerExitNoPress(_eventData);
                }
            }
        }


        /// <summary>
        /// 中断拖拽
        /// </summary>
        /// <param name="_isCallEnd">是否调用End事件</param>
        public void BreakDrag(bool _isCallEnd)
        {
            if (m_isDraging)
            {
                m_isDraging = false;
                m_dragIsBreaked = true;

                if (_isCallEnd)
                {
                    OnEndDrag(m_dragEventData);
                }
            }
        }

        private void SetHoveringDic(int _pointID, bool _hovering)
        {
            if (!m_curHoveringDic.ContainsKey(_pointID))
            {
                m_curHoveringDic.Add(_pointID, null);
            }

            if (_hovering)
            {
                m_curHoveringDic[_pointID] = this;
            }
            else
            {
                if (m_curHoveringDic[_pointID] == this)
                    m_curHoveringDic[_pointID] = null;
            }
        }

        private InputEventer GetCurHovering(int _pointID)
        {
            if (m_curHoveringDic.ContainsKey(_pointID))
            {
                return m_curHoveringDic[_pointID];
            }

            return null;
        }

        private void UpdateHoveringNoPressed(PointerEventData _data)
        {
            bool l_shouldHovering = m_isPressing;

            l_shouldHovering |= (m_isPressing && !m_isHovering && _data.pointerPress == gameObject)
                    || (!m_isPressing && m_isHovering && _data.pointerPress == gameObject)
                    || (!m_isPressing && m_isHovering && _data.pointerPress == null);

            if (m_isHoveringNoPressed != l_shouldHovering)
            {
                m_isHoveringNoPressed = l_shouldHovering;

                if (m_isHoveringNoPressed)
                {
                    OnPointerEnterNoPress(_data);
                }
                else
                {
                    OnPointerExitNoPress(_data);

                    if (!m_isHovering && !m_isPressing)
                    {
                        InputEventer l_curHover = GetCurHovering(_data.pointerId);

                        if (l_curHover != null && l_curHover != this)
                        {
                            if (l_curHover.m_isHovering && !l_curHover.m_isPressing && !l_curHover.m_isHoveringNoPressed)
                            {
                                l_curHover.m_isHoveringNoPressed = true;

                                l_curHover.OnPointerEnterNoPress(_data);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 获取InputEventer
        /// </summary>
        /// <param name="_go">GameObject</param>
        /// <returns></returns>
        public static InputEventer Get(GameObject _go)
        {
            InputEventer inputEventer = _go.GetComponent<InputEventer>();

            if (inputEventer == null)
                inputEventer = _go.AddComponent<InputEventer>();

            return inputEventer;
        }

        /// <summary>
        /// 获取InputEventer
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="_go">GameObject</param>
        /// <returns></returns>
        public static T Get<T>(GameObject _go) where T : InputEventer
        {
            T t = _go.GetComponent<T>();
            if (t == null)
                t = _go.AddComponent<T>();
            return t;
        }
    }
}