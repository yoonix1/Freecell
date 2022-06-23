using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    public RectTransform myParentRect;
    public DropZoneMode zoneMode;
    public int colIdx;

    private LinkedList<Card> stack = new LinkedList<Card>();
    private RectTransform rect;
    private Image img;

    public RectTransform GetPileRect() { return myParentRect; }
    public RectTransform GetRect() { return rect;  }
    public Card GetCard() { return stack.Last?.Value;  }
    public LinkedList<Card> GetStack() { return stack; }


    public Boolean IsOnDeck() { return zoneMode == DropZoneMode.Deck; }
    public Vector3 GetAnchorPos() { return myParentRect.anchoredPosition; }

    void Awake()
    {
        img = GetComponent<Image>();
        rect = GetComponent<RectTransform>();
        rect.sizeDelta = new Vector2(Constants.CARD_WIDTH + Constants.CARD_PADDING, Constants.CARD_HEIGHT + Constants.CARD_PADDING);
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
        Vector2 anchorPos = GetAnchorPos();
	    LinkedListNode<Card> item = moving.First;

        Deck.Instance.RecordHistory(moving, this);

	    while(item != null)
	    {
	        moving.RemoveFirst();

            Deck.Instance.OnCardMoved(item.Value.GetDropZone(), this);
	        stack.AddLast(item);
	        item.Value.SetCurrentDropZone(this);
            item = moving.First;
	    }
	}

    public void MoveTo(Card card) 
	{
        LinkedList<Card> list = card.GetDropZone().GetStack();
        LinkedListNode<Card> node = list.Last;

	    list.RemoveLast();

        Deck.Instance.OnCardMoved(card.GetDropZone(), this);
	    stack.AddLast(node);
	    card.SetCurrentDropZone(this);
    }

    public void Add(Card card)
    { 
	    stack.AddLast(card);
	    card.SetCurrentDropZone(this);
    }

    public bool IsLegalMove(Card c)
	{
	    DropZoneMode fromZone = c.GetDropZone().zoneMode;
	    CardFront card = (CardFront)c;
	    CardFront self = (CardFront)stack.Last?.Value;
        if (zoneMode == DropZoneMode.Deck)
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
