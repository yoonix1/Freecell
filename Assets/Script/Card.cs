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
        rect.sizeDelta = new Vector2(Constants.CARD_WIDTH, Constants.CARD_HEIGHT);
        canvas = FindFirstCanvas(rect);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Deck.Instance.IsCardMoving = true;
        //Debug.Log("BeginDrag" + isDropped);
        startingPos = GetRect().anchoredPosition;
        targetDropZone = null;

	    stackToMove = currentDropZone.GetCardsFrom(this);
        
        foreach(Card c in stackToMove)
        {
            c.GetRect().SetAsLastSibling();
            c.canvasGroup.alpha = 0.3f;
	    }

        // disable card/mouse interaction
        foreach(Card c in Deck.Instance.GetCards())
        {
            c.canvasGroup.blocksRaycasts = false;
	    }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        //Debug.Log("OnDrag" + isDropped);
        Vector2 amount = eventData.delta / canvas.scaleFactor;

        foreach( Card c in stackToMove )
        {
            c.GetRect().anchoredPosition += amount;
	    }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // enable card/mouse interaction
        foreach(Card c in Deck.Instance.GetCards())
        {
            c.canvasGroup.blocksRaycasts = true;
	    }

        foreach(Card c in stackToMove)
        {
            c.canvasGroup.alpha = 1.0f;
	    }

        Vector3 targetPosition = startingPos;
	    DropZoneMode zoneMode = currentDropZone.zoneMode;

        if (targetDropZone == null)
        { 
	        targetDropZone = currentDropZone;
	    }

        targetPosition = targetDropZone.GetAnchorPos();
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
        Deck.Instance.IsCardMoving = false;

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
