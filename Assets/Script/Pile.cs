using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

public class Pile : MonoBehaviour, IDropHandler
{
    public float deckVoffset = -Constants.CARD_PADDING_H;
    public Deck theDeck;
    public DropZoneMode zoneMode;
    public int colIdx;

    private Card myCard;
    private RectTransform myRect;

    void Awake()
    {
        myCard = GetComponent<Card>();
        myRect = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            Card dropzone = myCard;
            Card dropee = eventData.pointerDrag.GetComponent<Card>();

            if (CanBeDropped(dropzone, dropee))
            {
                Vector2 anchorPos = myRect.anchoredPosition;
                if (zoneMode == DropZoneMode.Deck && dropzone != null)
                {
                    anchorPos.y += deckVoffset;
                }

                eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = anchorPos;
                eventData.pointerDrag.GetComponent<RectTransform>().SetAsLastSibling();
                eventData.pointerDrag.GetComponent<Draggable>().isDropped = true;
                theDeck.UpdateDropZones(gameObject, eventData.pointerDrag);
            }
        }
    }

    bool CanBeDropped(Card dropzoneCard, Card dropee)
    {
        Pile dropeePile = dropee.GetComponent<Pile>();
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
