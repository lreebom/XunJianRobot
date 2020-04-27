using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

/// <summary>
/// <para> 功能 : 一些小功能函数
/// </para>
/// <para> 作者 : Liubo
/// </para>
/// <para> 日期 : 2014-03-13
/// </para>
///     Version :1
/// 
///     V1:初始版本
/// </summary>
namespace TRDev.SafetyClient
{
    /// <summary>
    /// 
    /// Version:1.0
    /// </summary>
    public sealed class Lreebom
    {
        private static float m_currentDPI = -1f;

        #region 公有变量

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBPLAYER
        [Obsolete("过时了")]
        public const InputPlatformType INPUT_PLATFORM = InputPlatformType.Mouse;
#elif UNITY_ANDROID || UNITY_IPHONE
        [Obsolete("过时了")]
        public const InputPlatformType INPUT_PLATFORM = InputPlatformType.Touch;
#endif

        /// <summary>
        /// Everything的LayerMask值
        /// </summary>
        public static int layerMaskEverything
        {
            get { return -1; }
        }

        /// <summary>
        /// Nothing的LayerMask值
        /// </summary>
        public static int layerMaskNothing
        {
            get { return 0; }
        }

        /// <summary>
        /// 当前DPI
        /// </summary>
        public static float currentDPI
        {
            get
            {
                if (m_currentDPI == -1f)
                {
                    float l_dpi = Screen.dpi;

                    if (l_dpi == 0f)
                    {
                        RuntimePlatform l_platform = Application.platform;

                        l_dpi = (l_platform == RuntimePlatform.Android || l_platform == RuntimePlatform.IPhonePlayer) ? 160f : 96f;
#if UNITY_BLACKBERRY
			            if (l_platform == RuntimePlatform.BB10Player) dpi = 160f;
#elif UNITY_WP8 || UNITY_WP_8_1
                        if (l_platform == RuntimePlatform.WP8Player) dpi = 160f;
#endif
                    }

                    m_currentDPI = l_dpi;
                }

                return m_currentDPI;
            }
        }

        #endregion

        #region Public Functions
        /// <summary>
        /// 限制浮点数，举个栗子:_min = 1f, _max = 5f，当_value = 5.123f时 返回1.123f，当_value = 0.7f时 返回4.3f。
        /// </summary>
        /// <param name="_value">源值</param>
        /// <param name="_min">最小</param>
        /// <param name="_max">最大</param>
        /// <returns>限制后的值</returns>
        public static float ClampFloat(float _value, float _min, float _max)
        {
            if (_min == _max)
            {
                Debug.Log("ClampEx 用法错误");
                return _min;
            }
            else if (_min > _max)
            {
                float l_temp = _min;
                _min = _max;
                _max = l_temp;
            }

            if (_value >= _min && _value <= _max)
            {
                return _value;
            }
            else if (_value < _min)
            {
                return ClampFloat(_max - (_min - _value), _min, _max);
            }
            else
            {
                return ClampFloat(_min + (_value - _max), _min, _max);
            }

        }

        /// <summary>
        /// 限制角度
        /// </summary>
        /// <param name="_angle">源角度</param>
        /// <returns>限制后的角度</returns>
        public static float ClampAngle(float _angle)
        {
            return ClampFloat(_angle, 0f, 360f);
        }

        /// <summary>
        /// 限制Vector3
        /// </summary>
        /// <param name="_vec">Vector3</param>
        /// <param name="_min">最小</param>
        /// <param name="_max">最大</param>
        /// <returns></returns>
        public static Vector3 ClampVector3(Vector3 _vec, Vector3 _min, Vector3 _max)
        {
            return new Vector3(Mathf.Clamp(_vec.x, _min.x, _max.x), Mathf.Clamp(_vec.y, _min.y, _max.y), Mathf.Clamp(_vec.z, _min.z, _max.z));
        }

        /// <summary>
        /// 获取角度
        /// </summary>
        /// <param name="_source">原值</param>
        /// <param name="_deg">度</param>
        /// <param name="_minute">分</param>
        public static void GetAngleFromFloat(float _source, out int _deg, out int _minute)
        {
            _deg = Mathf.FloorToInt(_source);
            float l_minuteFloat = _source - _deg;
            _minute = Mathf.RoundToInt(Mathf.Lerp(0f, 60f, l_minuteFloat));
        }

        /// <summary>
        /// LayerMask转Layer(LayerMask必须有且仅有一个值)
        /// </summary>
        public static int LayerMask2Layer(LayerMask _layerMask)
        {
            float l_layerF = Mathf.Log(_layerMask.value, 2f);

            int l_layer = (int)l_layerF;

            if (l_layer < 0 || l_layer > 31)
            {
                Debug.Log("LayerMask必须有且仅有一个值!");
                return 0;
            }
            if ((l_layerF - l_layer) != 0f)
            {
                Debug.Log("LayerMask必须有且仅有一个值!");
                return 0;
            }

            return l_layer;
        }

        /// <summary>
        /// Layer转LayerMask
        /// </summary>
        public static int Layer2LayerMask(int _layer)
        {
            if (_layer >= 0 && _layer <= 31)
            {
                return 1 << _layer;
            }
            else
            {
                Debug.Log("Layer超限");
                return 0;
            }
        }

        /// <summary>
        /// LayerMask是否包含Layer
        /// </summary>
        /// <param name="_layerMask"></param>
        /// <param name="_layer"></param>
        /// <returns></returns>
        public static bool LayerMaskContainLayer(int _layerMask, int _layer)
        {
            return _layerMask == (_layerMask | (1 << _layer));
        }

        /// <summary>
        /// 添加Layer
        /// </summary>
        /// <param name="_layerMask">LayerMask</param>
        /// <param name="_layer">Layer</param>
        /// <returns>LayerMask</returns>
        public static int LayerMaskAddLayer(int _layerMask, int _layer)
        {
            return _layerMask |= (1 << _layer);
        }

        /// <summary>
        /// 移除Layer
        /// </summary>
        /// <param name="_layerMask">LayerMask</param>
        /// <param name="_layer">Layer</param>
        /// <returns>LayerMask</returns>
        public static int LayerMaskRemoveLayer(int _layerMask, int _layer)
        {
            if (LayerMaskContainLayer(_layerMask, _layer))
            {
                return _layerMask ^= (1 << _layer);
            }
            else
            {
                return _layerMask;
            }
        }

        /// <summary>
        /// 重置Transform
        /// </summary>
        /// <param name="_target">Transform</param>
        public static void ReSetTranform(Transform _target)
        {
            _target.localPosition = Vector3.zero;
            _target.localRotation = Quaternion.identity;
            _target.localScale = Vector3.one;
        }

        /// <summary>
        /// 获取GameObject的RenderSize
        /// </summary>
        /// <param name="_go">GameObject</param>
        /// <returns>RenderSize</returns>
        public static float GetGameObjectRenderSize(GameObject _go)
        {
            return GetRenderersSize(_go.GetComponentsInChildren<Renderer>());
        }

        public static float GetGameObjectMeshRenderSize(GameObject _go)
        {
            return GetGameObjectMeshRenderSize(_go, true);
        }

        public static float GetGameObjectMeshRenderSize(GameObject _go, bool _getSkin)
        {
            if (!_getSkin)
            {
                return GetRenderersSize(_go.GetComponentsInChildren<MeshRenderer>());
            }
            else
            {
                System.Collections.Generic.List<Renderer> l_renderList = new System.Collections.Generic.List<Renderer>();
                MeshRenderer[] l_mrs = _go.GetComponentsInChildren<MeshRenderer>();
                if (l_mrs != null && l_mrs.Length > 0)
                {
                    l_renderList.AddRange(l_mrs);
                }

                SkinnedMeshRenderer[] l_skinMrs = _go.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (l_skinMrs != null && l_skinMrs.Length > 0)
                {
                    l_renderList.AddRange(l_skinMrs);
                }

                return GetRenderersSize(l_renderList.ToArray());
            }
        }

        /// <summary>
        /// 获取GameObject的RenderCenter
        /// </summary>
        /// <param name="_go">GameObject</param>
        /// <returns>RenderCenter</returns>
        public static Vector3 GetGameObjectRenderCenter(GameObject _go)
        {
            return GetRenderersCenter(_go.GetComponentsInChildren<Renderer>());
        }

        public static Vector3 GetGameObjectMeshRenderCenter(GameObject _go)
        {
            return GetGameObjectMeshRenderCenter(_go, true);
        }

        public static Vector3 GetGameObjectMeshRenderCenter(GameObject _go, bool _getSkin)
        {
            if (!_getSkin)
            {
                return GetRenderersCenter(_go.GetComponentsInChildren<Renderer>());
            }
            else
            {
                System.Collections.Generic.List<Renderer> l_renderList = new System.Collections.Generic.List<Renderer>();
                MeshRenderer[] l_mrs = _go.GetComponentsInChildren<MeshRenderer>();
                if (l_mrs != null && l_mrs.Length > 0)
                {
                    l_renderList.AddRange(l_mrs);
                }

                SkinnedMeshRenderer[] l_skinMrs = _go.GetComponentsInChildren<SkinnedMeshRenderer>();
                if (l_skinMrs != null && l_skinMrs.Length > 0)
                {
                    l_renderList.AddRange(l_skinMrs);
                }

                return GetRenderersCenter(l_renderList.ToArray());
            }
        }

        /// <summary>
        /// 获取RenderSize
        /// </summary>
        /// <param name="_renderers">Renderer数组</param>
        /// <returns>RenderSize</returns>
        public static float GetRenderersSize(Renderer[] _renderers)
        {
            Vector3 l_minValue = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            Vector3 l_maxValue = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (l_minValue.x > _renderers[i].bounds.min.x)
                    l_minValue.x = _renderers[i].bounds.min.x;
                if (l_minValue.y > _renderers[i].bounds.min.y)
                    l_minValue.y = _renderers[i].bounds.min.y;
                if (l_minValue.z > _renderers[i].bounds.min.z)
                    l_minValue.z = _renderers[i].bounds.min.z;

                if (l_maxValue.x < _renderers[i].bounds.max.x)
                    l_maxValue.x = _renderers[i].bounds.max.x;
                if (l_maxValue.y < _renderers[i].bounds.max.y)
                    l_maxValue.y = _renderers[i].bounds.max.y;
                if (l_maxValue.z < _renderers[i].bounds.max.z)
                    l_maxValue.z = _renderers[i].bounds.max.z;
            }

            return Vector3.Distance(l_minValue, l_maxValue);
        }

        /// <summary>
        /// 获取RenderCenter
        /// </summary>
        /// <param name="_renderers">Renderer数组</param>
        /// <returns>RenderCenter</returns>
        public static Vector3 GetRenderersCenter(Renderer[] _renderers)
        {
            Vector3 l_minValue = new Vector3(Mathf.Infinity, Mathf.Infinity, Mathf.Infinity);
            Vector3 l_maxValue = new Vector3(-Mathf.Infinity, -Mathf.Infinity, -Mathf.Infinity);

            for (int i = 0; i < _renderers.Length; i++)
            {
                if (l_minValue.x > _renderers[i].bounds.min.x)
                    l_minValue.x = _renderers[i].bounds.min.x;
                if (l_minValue.y > _renderers[i].bounds.min.y)
                    l_minValue.y = _renderers[i].bounds.min.y;
                if (l_minValue.z > _renderers[i].bounds.min.z)
                    l_minValue.z = _renderers[i].bounds.min.z;

                if (l_maxValue.x < _renderers[i].bounds.max.x)
                    l_maxValue.x = _renderers[i].bounds.max.x;
                if (l_maxValue.y < _renderers[i].bounds.max.y)
                    l_maxValue.y = _renderers[i].bounds.max.y;
                if (l_maxValue.z < _renderers[i].bounds.max.z)
                    l_maxValue.z = _renderers[i].bounds.max.z;
            }

            return l_minValue + (l_maxValue - l_minValue) * 0.5f;
        }

        /// <summary>
        /// String 转成 Vector3
        /// </summary>
        /// <param name="_str">String</param>
        /// <returns>Vector3</returns>
        public static Vector3 StringToVector3(string _str)
        {
            string[] l_strs = _str.Split(new char[] { 'x', 'y', 'z' });

            if (l_strs.Length == 3)
            {
                float l_x, l_y, l_z;
                if (float.TryParse(l_strs[0], out l_x) && float.TryParse(l_strs[1], out l_y) && float.TryParse(l_strs[2], out l_z))
                {
                    return new Vector3(l_x, l_y, l_z);
                }
                else
                {
                    Debug.Log("StringToVector3 出错");
                    return Vector3.zero;
                }
            }
            else
            {
                Debug.Log("StringToVector3 出错");
                return Vector3.zero;
            }

        }

        /// <summary>
        /// 保留float后面几位 四舍五入
        /// </summary>
        /// <param name="_value">要处理的值</param>
        /// <param name="_count">保留几位</param>
        /// <returns></returns>
        public static float FloatRound(float _value, int _count)
        {
            return (float)System.Math.Round(_value, _count);
        }

        /// <summary>
        /// 省略小数点
        /// </summary>
        /// <param name="_value">源值</param>
        /// <param name="_count">保留几位</param>
        /// <returns>结果值</returns>
        public static float FloatFloor(float _value, int _count)
        {
            float l_tenV = Mathf.Pow(10f, _count);
            return (Mathf.FloorToInt(_value * l_tenV)) / l_tenV;
        }

        /// <summary>
        /// 缩放
        /// </summary>
        /// <param name="_type">类型</param>
        /// <param name="_srcSize">源Size</param>
        /// <param name="_boundsSize">BoundSize</param>
        /// <returns>结果</returns>
        public static Vector2 ScaleSize(ScaleSizeType _type, Vector2 _srcSize, Vector2 _boundsSize)
        {
            Vector2 l_targetSize = Vector2.zero;

            float l_srcRatio = _srcSize.x / _srcSize.y;

            float l_boundsRatio = _boundsSize.x / _boundsSize.y;

            if (l_srcRatio != l_boundsRatio)
            {
                switch (_type)
                {
                    case ScaleSizeType.适应:
                        if (l_srcRatio > l_boundsRatio)
                        {
                            l_targetSize.x = _boundsSize.x;
                            l_targetSize.y = _boundsSize.x / l_srcRatio;
                        }
                        else
                        {
                            l_targetSize.x = _boundsSize.y * l_srcRatio;
                            l_targetSize.y = _boundsSize.y;
                        }
                        break;

                    case ScaleSizeType.拉伸:
                        l_targetSize = _boundsSize;
                        break;

                    case ScaleSizeType.填充:
                        if (l_srcRatio > l_boundsRatio)
                        {
                            l_targetSize.x = _boundsSize.y * l_srcRatio;
                            l_targetSize.y = _boundsSize.y;
                        }
                        else
                        {
                            l_targetSize.x = _boundsSize.x;
                            l_targetSize.y = _boundsSize.x / l_srcRatio;
                        }
                        break;
                }
            }
            else
            {
                if (_srcSize.x > _boundsSize.x)
                {
                    l_targetSize = _boundsSize;
                }
                else
                {
                    l_targetSize = _srcSize;
                }
            }

            return l_targetSize;
        }

        /// <summary>
        /// 通过TriangleIndex获取MaterialID(需要遍历Mesh的三角形数组，比较耗性能)
        /// </summary>
        /// <param name="_mesh"></param>
        /// <param name="_triangleIndex"></param>
        /// <returns></returns>
        public static int GetMaterialIDByTriangleIndex(Mesh _mesh, int _triangleIndex)
        {
            int[] l_triangles = _mesh.triangles;

            int l_id0 = l_triangles[_triangleIndex * 3 + 0];
            int l_id1 = l_triangles[_triangleIndex * 3 + 1];
            int l_id2 = l_triangles[_triangleIndex * 3 + 2];

            int l_meshCount = _mesh.subMeshCount;

            int l_materialID = -1;

            for (int l_i = 0; l_i < l_meshCount; l_i++)
            {
                int[] l_subTriangles = _mesh.GetTriangles(l_i);

                for (int l_j = 0; l_j < l_subTriangles.Length; l_j += 3)
                {
                    if (l_subTriangles[l_j] == l_id0 && l_subTriangles[l_j + 1] == l_id1 && l_subTriangles[l_j + 2] == l_id2)
                    {
                        l_materialID = l_i;
                        break;
                    }
                }

                if (l_materialID != -1) break;
            }

            return l_materialID;
        }

        /// <summary>
        /// 获取或添加组件
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="go">GameObject</param>
        /// <returns>组件</returns>
        public static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            T l_comp = go.GetComponent<T>();
            if (!l_comp)
                l_comp = go.AddComponent<T>();
            return l_comp;
        }

        #endregion

        #region Delegate
        public delegate void DelegateVoid();
        public delegate void DelegateBool(bool _isTrue);
        public delegate void DelegateInt(int _value);
        public delegate void DelegateFloat(float _value);
        public delegate void DelegateString(string _value);
        public delegate void DelegateVector2(Vector2 _value);
        public delegate void DelegateVector3(Vector3 _value);
        public delegate void DelegateData(object _data);

        public delegate void DelegateGameObject(GameObject _go);
        public delegate void DelagateGameObjectBool(GameObject _go, bool _isTrue);
        public delegate void DelegateGameObjectInt(GameObject _go, int _value);
        public delegate void DelegateGameObjectFloat(GameObject _go, float _value);
        public delegate void DelegateGameObjectString(GameObject _go, string _value);
        public delegate void DelegateGameObjectVector2(GameObject _go, Vector2 _value);
        public delegate void DelegateGameObjectVector3(GameObject _go, Vector3 _value);
        public delegate void DelegateGameObjectData(GameObject _go, object _data);


        #endregion

        #region Events
        public class OnVoidEvent : UnityEngine.Events.UnityEvent { }
        public class OnBooleanEvent : UnityEngine.Events.UnityEvent<bool> { }
        public class OnIntegerEvent : UnityEngine.Events.UnityEvent<int> { }
        public class OnFloatEvent : UnityEngine.Events.UnityEvent<float> { }
        public class OnColorEvent : UnityEngine.Events.UnityEvent<Color> { }
        public class OnVector2Event : UnityEngine.Events.UnityEvent<Vector2> { }
        public class OnVector3Event : UnityEngine.Events.UnityEvent<Vector3> { }
        public class OnStringEvent : UnityEngine.Events.UnityEvent<string> { }
        #endregion

        public enum UpdateType
        {
            None,
            Update,
            LateUpdate,
        }

        public enum ScaleSizeType
        {
            适应,
            填充,
            拉伸,
        }

        public enum InputPlatformType
        {
            Mouse,
            Touch,
        }
    }

}