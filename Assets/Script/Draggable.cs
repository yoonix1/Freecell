using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform rect;
    public Canvas canvas;
    void Awake()
    {
        rect = gameObject.GetComponent<RectTransform>();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("StartDrag");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("EndDrag");
    }
}
