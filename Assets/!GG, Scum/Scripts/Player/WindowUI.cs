using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WindowUI : MonoBehaviour, IDragHandler, IPointerDownHandler
{
    Vector3 MouseDragStartPos;

    public void OnDrag(PointerEventData eventData)
    {
        transform.localPosition = Input.mousePosition - MouseDragStartPos;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        MouseDragStartPos = Input.mousePosition - transform.localPosition;
    }
}