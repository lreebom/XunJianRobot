using UnityEngine;
using System.Collections;

namespace TRDev.SafetyClient
{
    [RequireComponent(typeof(InputEventDrager))]
    public class InputEventDragerLimit : MonoBehaviour
    {
        public LimitType limitType = LimitType.None;

        public Space space = Space.World;

        public Vector3 minPosition;
        public Vector3 maxPosition;

        InputEventDrager m_drager;

        void Start()
        {
            m_drager = GetComponent<InputEventDrager>();
            if (m_drager)
                m_drager.onDrag.AddListener(OnDrag);
        }

        void OnDrag(Transform _target)
        {
            switch (limitType)
            {
                case LimitType.None:
                    return;
                case LimitType.MinMaxPosition:
                    if (space == Space.World)
                    {
                        _target.position = Lreebom.ClampVector3(_target.position, minPosition, maxPosition);
                    }
                    else if (space == Space.Self)
                    {
                        _target.localPosition = Lreebom.ClampVector3(_target.localPosition, minPosition, maxPosition);
                    }
                    break;

                default:
                    break;
            }
        }

        public enum LimitType
        {
            None,
            MinMaxPosition,
        }
    }
}