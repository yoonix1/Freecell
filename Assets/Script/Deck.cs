using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public class Deck : MonoBehaviour
{
    public Button deal;

    public Pile objPile;
    public Card objCard;
    
    private Card[] card = new Card[Constants.NUMBER_OF_CARDS];

    private LinkedList<Card>[] column = new LinkedList<Card>[8];
    private Pile[] pile = new Pile[4];
    private Pile[] work = new Pile[4];
    private Pile[] colPile = new Pile[8];
    
    private Sprite[] sprites = new Sprite[Constants.NUMBER_OF_CARDS];

    private float width = 0;
    private float height = 0;
    
    void Start()
    {
        int i;
        deal.onClick.AddListener(OnDeal);
        
        sprites = Resources.LoadAll<Sprite>("Textures/Card");

        RectTransform rect = GetComponent<RectTransform>();
        width = rect.rect.width;
        height = rect.rect.height;

        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i] = Instantiate(objCard, Vector3.zero, Quaternion.identity, rect);
            card[i].SetValue(i);
        }

        for (i = 0; i < 8; i++)
        {
            column[i] = new LinkedList<Card>();
        }

        float colwidth = Constants.CARD_PADDING_W;
        float offset = Constants.SCREEN_OFFSET_W - 1;
        for (i = 0; i < 4; i++)
        {
            pile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            pile[i].GetComponent<RectTransform>().anchoredPosition = new Vector2( colwidth * (i + 4) + offset, -100);
            pile[i].zoneMode = DropZoneMode.Pile;
            
            work[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            work[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(colwidth * i + offset, -100);
            work[i].zoneMode = DropZoneMode.Work;
        }

        for (i = 0; i < 8; i++)
        {
            colPile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            colPile[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(colwidth * i + offset , Constants.SCREEN_OFFSET_H);
            colPile[i].zoneMode = DropZoneMode.Deck;
        }
    }



    private void OnDeal()
    {
        //deal.gameObject.SetActive(false);
        DoDeal(10);
        DrawDeck();
        MarkAllCardsOnDeck();
    }
    private void OnGameOver()
    {
        deal.gameObject.SetActive(true);
    }
    public Sprite GetSprite(int idx)
    {
        return sprites[idx];
    }

    public void DrawDeck()
    {
        RectTransform myRect = GetComponent<RectTransform>();
        Vector2 offset = Vector2.zero;
        int i = 0;
        int icol = 0;
        foreach (LinkedList<Card> cards in column)
        {
            offset.x = icol * Constants.CARD_PADDING_W + Constants.SCREEN_OFFSET_W;
            i = 0;
            foreach (Card item in cards)
            {
                offset.y = -Constants.CARD_PADDING_H * i + Constants.SCREEN_OFFSET_H;
                item.GetComponent<RectTransform>().anchoredPosition = offset;
                item.GetComponent<RectTransform>().SetAsLastSibling();
                item.GetComponent<Draggable>().enabled = false;
                if (cards.Last.Value == item)
                {
                    item.GetComponent<Draggable>().enabled = true;
                }
                i += 1;
            }
            icol += 1;
        }

	for(i = 0; i < 4; i++)
	{
            pile[i].GetComponent<Pile>().enabled = true;
	    work[i].GetComponent<Pile>().enabled = true;
	}
    }

    public void UpdateDropZones(GameObject to, GameObject moved)
    {
        Pile dropee = moved.GetComponent<Pile>();
        if (dropee)
        {
            if (dropee.zoneMode == DropZoneMode.Deck)
            {
                if (column[dropee.colIdx].Last != null)
                {
                    column[dropee.colIdx].RemoveLast();
                    if (column[dropee.colIdx].Last != null)
                    {
                        column[dropee.colIdx].Last.Value.GetComponent<Draggable>().enabled = true;
		    }
                }
            }
        }

        Pile dropzone = to.GetComponent<Pile>();
        if (dropzone)
        {
            if (dropzone.zoneMode == DropZoneMode.Deck)
            {
                dropee.colIdx = dropzone.colIdx;
                dropee.GetComponent<Draggable>().enabled = true;
                dropee.zoneMode = DropZoneMode.Deck;
                if (column[dropzone.colIdx].Last != null)
                {
                    column[dropzone.colIdx].Last.Value.GetComponent<Draggable>().enabled = false;
                    column[dropzone.colIdx].AddLast(dropee.GetComponent<Card>());
                }
            }
            else if (dropzone.zoneMode == DropZoneMode.Pile)
            {
                dropzone.enabled = false;
                dropee.enabled = true; 
                //dropee.GetComponent<Draggable>().enabled = false;
                //dropee.GetComponent<CanvasGroup>().blocksRaycasts = false;
                dropee.zoneMode = DropZoneMode.Pile;
            }
            else if (dropzone.zoneMode == DropZoneMode.Work)
            {
                dropee.zoneMode = DropZoneMode.Work;
            }
        }
    }

    private void MarkAllCardsOnDeck()
    {
        int idx = 0;
        foreach (LinkedList<Card> col in column)
        {
            foreach (Card card in col)
            {
                Pile pile = card.GetComponent<Pile>();
                pile.zoneMode = DropZoneMode.Deck;
                pile.colIdx = idx;
            }
            idx++;
        }
    }

    private void DoDeal(uint seed)
    {
        int i;
        uint c;
        uint cardsleft = Constants.NUMBER_OF_CARDS;
        
        foreach(LinkedList<Card> col in column)
        {
            col.Clear();
        }
        
        int[] cards = new int[Constants.NUMBER_OF_CARDS];
        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            cards[i]  = i;   
        }
        
        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            seed = (seed * 214013 + 2531011) & 0xffffffff;
            c = ((seed >> 16) & 0x7fff) % cardsleft;
            column[i % 8].AddLast(card[cards[c]]);
            cards[c] = cards[cardsleft - 1];
            cardsleft -= 1;
        }
    }
}
