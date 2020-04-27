
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PressButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public class OnPress : UnityEngine.Events.UnityEvent<bool> { }

    public OnPress onPress = new OnPress();

    public void OnPointerDown(PointerEventData eventData)
    {
        onPress.Invoke(true);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        onPress.Invoke(false);
    }
}
