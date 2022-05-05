using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    public Deck theDeck;
    public DropZoneMode zoneMode;
    public int colIdx;

    // all cards are dropzones since we can put a card on top of card.
    private Card self;
    private RectTransform rect;
    private Image img;

    public RectTransform GetRect() { return rect;  }
    public Card GetCard() { return self;  }

    public void SetColor(Color c) { img.color = c; }

    public Boolean IsOnDeck() { return zoneMode == DropZoneMode.Deck || zoneMode == DropZoneMode.Sorted; }

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        self = GetComponent<Card>();
        rect.sizeDelta = new Vector2(Constants.CARD_WIDTH, Constants.CARD_HEIGHT);
        img = GetComponent<Image>();
    }

    public void SetCard(Card card)
    {
        self = card;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            Card dropee = eventData.pointerDrag.GetComponent<Card>();

            if (theDeck.CanBeDropped(zoneMode, self, dropee))
            {
                Vector2 anchorPos = rect.anchoredPosition;
                if ((zoneMode == DropZoneMode.Deck || zoneMode == DropZoneMode.Sorted) && self != null)
                {
                    anchorPos.y += -Constants.PILE_OFFSET;
                }

                dropee.GetRect().anchoredPosition = anchorPos;
                dropee.GetRect().SetAsLastSibling();
                dropee.GetDraggable().isDropped = true;
                theDeck.UpdateDropZones(this, dropee.GetDraggable());
            }
        }
    }
}
