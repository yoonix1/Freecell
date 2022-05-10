using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public RectTransform GetRect() { return rect; }

    private Canvas canvas;
    private RectTransform rect;
    private DropZone targetDropZone; 
    private DropZone currentDropZone; 
    private CanvasGroup canvasGroup;

    private Vector2 startingPos;
    private LinkedList<Card> stackToMove;

    public LinkedList<Card> GetStack() { return stackToMove; }
    public bool IsStackMove() { return (stackToMove != null && stackToMove.Count > 1); }
    public void SetReadyToDrop(DropZone dropHere) { targetDropZone = dropHere; }
    public void SetCurrentDropZone(DropZone dropHere) { currentDropZone = dropHere; }
    public DropZone GetDropZone() { return currentDropZone; }
    public LinkedList<Card> GetCurrentPile() { return currentDropZone?.GetStack();}

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
	    rect = GetComponent<RectTransform>();
        canvas = FindFirstCanvas(rect);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        //Debug.Log("BeginDrag" + isDropped);
        startingPos = GetRect().anchoredPosition;
        targetDropZone = null;

	    stackToMove = currentDropZone.GetCardsFrom(this);
        
        foreach(Card c in stackToMove)
        {
            GetRect().SetAsLastSibling();
            c.canvasGroup.alpha = 0.3f;
	        c.canvasGroup.blocksRaycasts = false;
	    }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag" + isDropped);
        Vector2 amount = eventData.delta / canvas.scaleFactor;
        GetRect().anchoredPosition += amount;

        if (stackToMove != null)
        { 
            foreach( Card c in stackToMove )
            {
                c.GetRect().anchoredPosition += amount;
	        }
	    }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        foreach(Card c in stackToMove)
        {
            c.canvasGroup.alpha = 1.0f;
	        c.canvasGroup.blocksRaycasts = true;
	    }

        Vector3 targetPosition = startingPos;
	    DropZoneMode zoneMode = currentDropZone.zoneMode;

        if (targetDropZone == null)
        { 
	        targetDropZone = currentDropZone;
	    }

	    targetPosition = currentDropZone.GetRect().anchoredPosition;
	    targetDropZone.MoveTo(stackToMove);

	    if (targetDropZone.IsOnDeck()) 
	    {
	        LinkedList<Card> cards = targetDropZone.GetStack();
	        int i =0;
	        foreach(Card c in cards)
	        {
		        c.GetRect().anchoredPosition = new Vector3( targetPosition.x, targetPosition.y + i*( -Constants.PILE_OFFSET),  targetPosition.z);
		        i ++;
	        }
	    }
	    else 
	    {
            targetDropZone.GetStack().Last.Value.GetRect().anchoredPosition = targetPosition;
	    }
        //card.theDeck.PlaySound(SoundEffect.CardDropped);
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

    private void Removed()
	{
        if (currentDropZone.zoneMode == DropZoneMode.Work)
        {
            //Deck.Instance.numAvailableSlots++;
        }
        if (currentDropZone.zoneMode == DropZoneMode.Pile)
        {
            //currentDropZone.enabled = true;
        }
    }

    private void AddedTo(DropZone pile)
	{
        currentDropZone = pile; 	
	}


}
