using UnityEngine;
using System.Collections;

namespace TRDev.SafetyClient
{
    /// <summary>
    /// 
    /// Version:1.2
    /// v1.2 2015-09-09
    /// 更新内容：事件改为onChange， 继承自MonoBehaviourSingleton
    /// </summary>
    public class ResolutionListener : MonoBehaviourSingleton<ResolutionListener>
    {
        [SerializeField]
        private Lreebom.UpdateType m_updateType = Lreebom.UpdateType.LateUpdate;

        private int delayFrameCount = 2;

        public class OnResolutionChangeEvent : UnityEngine.Events.UnityEvent<int, int> { }

        /// <summary>
        /// 不要再Awake函数里添加事件
        /// </summary>
        public static OnResolutionChangeEvent onResolutionChange = new OnResolutionChangeEvent();

        private int m_lastWidth = 0;
        private int m_lastHeight = 0;

        protected override void Awake()
        {
            base.Awake();

            onResolutionChange.RemoveAllListeners();
        }

        protected override void Start()
        {
            base.Start();

            m_lastWidth = Screen.width;
            m_lastHeight = Screen.height;
        }

        void Update()
        {
            if (m_updateType == Lreebom.UpdateType.Update)
            {
                CalculateResolution();
            }
        }

        void LateUpdate()
        {
            if (m_updateType == Lreebom.UpdateType.LateUpdate)
            {
                CalculateResolution();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            onResolutionChange.RemoveAllListeners();
        }

        void CalculateResolution()
        {
            int l_curScreenWidth = Screen.width;
            int l_curScreenHeight = Screen.height;

            if (l_curScreenWidth != m_lastWidth || l_curScreenHeight != m_lastHeight)
            {
                m_lastWidth = l_curScreenWidth;
                m_lastHeight = l_curScreenHeight;

                if (delayFrameCount <= 0)
                {
                    if (onResolutionChange != null)
                        onResolutionChange.Invoke(l_curScreenWidth, l_curScreenHeight);
                }
                else
                {
                    StopAllCoroutines();

                    StartCoroutine(this.IE_OnScreenResolutionChange());
                }
            }
        }

        IEnumerator IE_OnScreenResolutionChange()
        {
            int l_curDelayCount = delayFrameCount;

            while (l_curDelayCount > 0)
            {
                l_curDelayCount--;
                yield return new WaitForEndOfFrame();
            }

            if (onResolutionChange != null)
                onResolutionChange.Invoke(Screen.width, Screen.height);
        }
    }
}