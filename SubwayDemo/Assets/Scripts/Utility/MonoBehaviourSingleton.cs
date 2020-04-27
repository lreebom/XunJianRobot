using UnityEngine;

namespace TRDev.SafetyClient
{
    /// <summary>
    /// 单例模板类
    /// </summary>
    /// <typeparam name="T">类</typeparam>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviourSingleton<T>
    {
        protected static T m_current = null;

        /// <summary>
        /// 当前实例化的对象
        /// </summary>
        public static T current
        {
            get
            {
                return m_current;
            }
        }

        protected virtual void Awake()
        {
            if (m_current == null)
                m_current = this as T;
            else
                Debug.Log("场景中有多个 " + this.name);
        }

        protected virtual void Start()
        {

        }

        protected virtual void OnDestroy()
        {
            if (m_current == this)
                m_current = null;
        }
    }
}