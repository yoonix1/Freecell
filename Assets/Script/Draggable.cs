using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler

{
    public Canvas canvas;
    public bool isDropped; // if dropped to a valid location other wise we will force back to starting achor position


    [System.NonSerialized]
    public Card card;

    private CanvasGroup canvasGroup;
    private Vector2 startingPos;
    
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        card = GetComponent<Card>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        startingPos = card.rect.anchoredPosition;
        isDropped = false;
        
        card.rect.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        card.rect.anchoredPosition += eventData.delta / canvas.scaleFactor;
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
            card.rect.anchoredPosition = startingPos;
        }
    }

}
