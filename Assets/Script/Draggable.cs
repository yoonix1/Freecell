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

    private LinkedList<Card> stackToMove;

    public LinkedList<Card> GetStack()
    {
        return stackToMove;
    }

    public bool IsStackMove()
    {
        return (stackToMove != null && stackToMove.Count > 0);
    }

    public void Dropped(int colIdx)
    {
        canvasGroup.alpha = 1.0f;
        card.GetDropZone().colIdx = colIdx;
    }

    
    void Awake()
    {
        canvasGroup = gameObject.GetComponent<CanvasGroup>();
        card = GetComponent<Card>();
        canvas = FindFirstCanvas(card.GetRect());
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("BeginDrag" + isDropped);
        startingPos = card.GetRect().anchoredPosition;
        isDropped = false;
        
        card.GetRect().SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.3f;

        if (card.GetDropZone().zoneMode == DropZoneMode.Sorted)
        {
            stackToMove = card.theDeck.GetColumnAfter(card);
            foreach(Card c in stackToMove)
            {
                c.GetRect().SetAsLastSibling();
                c.GetDraggable().canvasGroup.alpha = 0.3f;
	        }
            Debug.Log("Added Stack" + stackToMove.Count);
	    }
        else
        {
            stackToMove = null;
	    }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("OnDrag" + isDropped);
        Vector2 amount = eventData.delta / canvas.scaleFactor;
        card.GetRect().anchoredPosition += amount;

        if (stackToMove != null)
        { 
            Debug.Log("OnDrag" + stackToMove.Count);
            foreach( Card c in stackToMove )
            {
                c.GetRect().anchoredPosition += amount;
	        }
	    }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("EndDrag" + isDropped);
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1.0f;

        if (!isDropped)
        { 
            card.GetRect().anchoredPosition = startingPos;
            card.MoveStackHere(stackToMove);
            if (stackToMove != null)
            {
                foreach (Card c in stackToMove)
                {
                    c.GetDraggable().canvasGroup.alpha = 1.0f;
                }
            }
	    }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
    }
    
    public void OnPointerUp(PointerEventData eventData)
    {
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
