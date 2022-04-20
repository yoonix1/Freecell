using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class DropZone : MonoBehaviour, IDropHandler
{
    public float deckVoffset = -Constants.CARD_PADDING_H;
    public Deck theDeck;
    public DropZoneMode zoneMode;
    public int colIdx;

    // all cards are dropzones since we can put a card on top of card.
    [System.NonSerialized]
    public Card self;

    [System.NonSerialized]
    public RectTransform rect;

    [System.NonSerialized]
    public Card lastCard;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        self = GetComponent<Card>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            Card dropee = eventData.pointerDrag.GetComponent<Card>();

            if (CanBeDropped(self, dropee))
            {
                Vector2 anchorPos = rect.anchoredPosition;
                if (zoneMode == DropZoneMode.Deck && self != null)
                {
                    anchorPos.y += deckVoffset;
                }

                dropee.rect.anchoredPosition = anchorPos;
                dropee.rect.SetAsLastSibling();
                dropee.draggable.isDropped = true;
                theDeck.UpdateDropZones(gameObject, eventData.pointerDrag);
            }
        }
    }

    bool CanBeDropped(Card dropzoneCard, Card dropee)
    {
        DropZone dropeePile = dropee.GetComponent<DropZone>();
        if (zoneMode == DropZoneMode.Deck)
        {
            if (dropzoneCard == null)
            {
                return true;
            }
            if (dropzoneCard.value == dropee.value + 1)
            {
                if ((dropzoneCard.suit == Suit.CLUBS || dropzoneCard.suit == Suit.SPADE)
                    && (dropee.suit == Suit.DIAMOND || dropee.suit == Suit.HEART))
                {
                    return true;
                }
                if ((dropzoneCard.suit == Suit.DIAMOND || dropzoneCard.suit == Suit.HEART)
                         && (dropee.suit == Suit.CLUBS || dropee.suit == Suit.SPADE))
                {
                    return true;
                }
                return false;
            }
            return false;
        }
        else if (zoneMode == DropZoneMode.Work)
        {
	        if (dropeePile.zoneMode == DropZoneMode.Pile) 
	        {
	         	return false;
	        }

            if (dropzoneCard == null)
            {
                return true;
            }
            return false;
        }
        else if (zoneMode == DropZoneMode.Pile)
        {
            if (dropzoneCard == null)
            {
                if (dropee.value == 1)
                {
                    return true;
                }
            }
            else
            {
                if (dropzoneCard.value + 1 == dropee.value && dropzoneCard.suit == dropee.suit)
                {
                    return true;
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
