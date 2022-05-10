using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    public Deck theDeck;
    public DropZoneMode zoneMode;
    public int colIdx;

    private LinkedList<Card> stack = new LinkedList<Card>();
    private RectTransform rect;
    private Image img;

    public RectTransform GetRect() { return rect;  }
    public Card GetCard() { return stack.Last?.Value;  }
    public LinkedList<Card> GetStack() { return stack; }

    public void SetColor(Color c) { img.color = c; }

    public Boolean IsOnDeck() { return zoneMode == DropZoneMode.Deck || zoneMode == DropZoneMode.Sorted; }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Constants.CARD_WIDTH, Constants.CARD_HEIGHT);
        img = GetComponent<Image>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            Card dropee = eventData.pointerDrag.GetComponent<Card>();

            if (IsLegalMove(dropee))
            {
		        dropee.SetReadyToDrop(this);
            }
        }
    }

    public LinkedList<Card> GetCardsFrom(Card c)
    {
        LinkedList<Card> result = new LinkedList<Card>();
	    LinkedList<Card> currentList = c.GetCurrentPile();

        LinkedListNode<Card> toMove = c.GetCurrentPile().Last;

        while (toMove != null)
        {
            LinkedListNode<Card> item = toMove.Previous;

	        currentList.Remove(toMove);
	        result.AddFirst(toMove);

	        if ( toMove.Value == c ) 
		    {
		        toMove = null;
		    }
	        else
		    {
		        toMove = item;
		    }
        }

        return result;
    }

    public void MoveTo(LinkedList<Card> moving)
	{
        Vector2 anchorPos = rect.anchoredPosition;
	    while(moving.First != null)
	    {
	        LinkedListNode<Card> item = moving.First;
	    
            //item.Removed();

	        moving.RemoveFirst();
	        stack.AddLast(item);
	        item.Value.SetCurrentDropZone(this);

	    }
	}

    public void MoveTo(Card card) 
	{
	    stack.AddLast(card);
	    card.SetCurrentDropZone(this);
    }

    private bool IsLegalMove(Card c)
	{
	    DropZoneMode fromZone = c.GetDropZone().zoneMode;
	    CardFront card = (CardFront)c;
	    CardFront self = (CardFront)stack.Last?.Value;
        if (zoneMode == DropZoneMode.Deck || zoneMode == DropZoneMode.Sorted)
        {
            if (fromZone == DropZoneMode.Pile)
            {
                return false;
            }

            if (stack.Last == null) // I am a blank dropzone like last empty space on deck
            {
                return true;
            }
	        else if (self.value == card.value + 1)
            {
                if ((self.IsBlack() && card.IsRed()) || (self.IsRed() && card.IsBlack()))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        else if (zoneMode == DropZoneMode.Work)
        {
            if (fromZone == DropZoneMode.Pile)
            {
                return false;
            }
            if (self == null)
            {
                return !card.IsStackMove();
            }
            return false;
        }
        else if (zoneMode == DropZoneMode.Pile)
        {
            if (self == null)
            {
                if (card.value == 1)
                {
                    return !card.IsStackMove();
                }
            }
            else
            {
                if (self.value + 1 == card.value && self.suit == card.suit)
                {
                    return !card.IsStackMove();
                }
            }
            return false;
        }
        else
        {
            return false;
        }
    }
}
