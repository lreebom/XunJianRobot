using UnityEngine;
using UnityEngine.EventSystems;

namespace TRDev.SafetyClient
{
    /// <summary>
    /// 向上冒泡所有DragEvent
    /// </summary>
    public sealed class EventBubblerAllDrag : MonoBehaviour, IInitializePotentialDragHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public void OnInitializePotentialDrag(PointerEventData _eventData)
        {
            if (transform.parent != null)
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.initializePotentialDrag);
        }
        public void OnBeginDrag(PointerEventData _eventData)
        {
            if (transform.parent != null)
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.beginDragHandler);
        }

        public void OnDrag(PointerEventData _eventData)
        {
            if (transform.parent != null)
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.dragHandler);
        }

        public void OnEndDrag(PointerEventData _eventData)
        {
            if (transform.parent != null)
                ExecuteEvents.ExecuteHierarchy(transform.parent.gameObject, _eventData, ExecuteEvents.endDragHandler);
        }
    }
}