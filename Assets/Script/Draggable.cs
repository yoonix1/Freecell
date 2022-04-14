using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler

{
    public Canvas canvas;
    public bool isDropped; // if dropped to a valid location other wise we will force back to starting achor position
    private CanvasGroup canvasGroup;
    private RectTransform rect;
    private Vector2 startingPos;
    private bool reqDisable;
    
    void Awake()
    {
        rect = gameObject.GetComponent<RectTransform>();
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        startingPos = rect.anchoredPosition;
        isDropped = false;
        
        rect.SetAsLastSibling();
        
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        canvasGroup.alpha = 0.3f;
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;
        // i was just let go not dropped to a locked position
        if (!isDropped)
        {
            rect.anchoredPosition = startingPos;
        }
    }

}
