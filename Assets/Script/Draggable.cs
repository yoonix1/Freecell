using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Draggable : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
{
    public Canvas canvas;
    public bool isDropped; // if dropped to a valid location other wise we will force back to starting achor position

    private Card card;

    private CanvasGroup canvasGroup;
    private Vector2 startingPos;
    
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        card = GetComponent<Card>();
        canvas = FindFirstCanvas(card.GetRect());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startingPos = card.GetRect().anchoredPosition;
        isDropped = false;
        
        card.GetRect().SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        card.GetRect().anchoredPosition += eventData.delta / canvas.scaleFactor;
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
            card.GetRect().anchoredPosition = startingPos;
        }

        card.theDeck.PlaySound(SoundEffect.CardDropped);
    }

    private Canvas FindFirstCanvas(RectTransform rect)
    {
        while(rect!= null)
        {
            Canvas rtn;
            if (rect.TryGetComponent<Canvas>(out rtn))
            {
                return rtn;
            }
            rect = rect.parent.GetComponent<RectTransform>();
	    }

        return null;
    }


}
