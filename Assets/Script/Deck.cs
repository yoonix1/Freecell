using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using Debug = UnityEngine.Debug;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Deck : MonoBehaviour
{
    public Button deal;

    public DropZone objPile;
    public Card objCard;


    private Card[] card = new Card[Constants.NUMBER_OF_CARDS];

    private LinkedList<Card>[] column = new LinkedList<Card>[8];
    private DropZone[] pile = new DropZone[4];
    private DropZone[] work = new DropZone[4];
    private DropZone[] colPile = new DropZone[8];
    
    private Sprite[] sprites = new Sprite[Constants.NUMBER_OF_CARDS];

    [SerializeField]
    private AssetReference[] cardFronts;

    private AsyncOperationHandle resourceHandle;


    private int cardidx = 0;

    private float width = 0;
    private float height = 0;

    private void Awake()
    {
        int i;
        RectTransform rect = GetComponent<RectTransform>();
        width = rect.rect.width;
        height = rect.rect.height;

        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i] = Instantiate(objCard, Vector3.zero, Quaternion.identity, rect);
        }

        for (i = 0; i < 4; i++)
        {
            pile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            work[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
	    }

        for (i = 0; i < 8; i++)
        {
            colPile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            column[i] = new LinkedList<Card>();
        }
    }

    void Start()
    {
        int i;
        deal.onClick.AddListener(OnDeal);


        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i].SetValue(i);
        }

        float colwidth = Constants.CARD_PADDING_W;
        float offset = Constants.SCREEN_OFFSET_W - 1;
        for (i = 0; i < 4; i++)
        {
            pile[i].GetRect().anchoredPosition = new Vector2(colwidth * (i + 4) + offset, -100);
            pile[i].zoneMode = DropZoneMode.Pile;
            pile[i].colIdx = i;

            work[i].GetRect().anchoredPosition = new Vector2(colwidth * i + offset, -100);
            work[i].zoneMode = DropZoneMode.Work;
            work[i].colIdx = i;
        }

        for (i = 0; i < 8; i++)
        {
            colPile[i].GetRect().anchoredPosition = new Vector2(colwidth * i + offset, Constants.SCREEN_OFFSET_H);
            colPile[i].zoneMode = DropZoneMode.Deck;
            colPile[i].colIdx = i;
        }
    }

    private void OnDeal()
    {
        //deal.gameObject.SetActive(false);
        ChangeCardFront();
    }

    private void ContinueDealing()
    {
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
                item.GetRect().anchoredPosition = offset;
                item.GetRect().SetAsLastSibling();
                item.GetDraggable().enabled = false;
                if (cards.Last.Value == item)
                {
                    item.GetComponent<Draggable>().enabled = true;
                }
                i += 1;
            }
            icol += 1;
        }
    }

    public void UpdateDropZones(GameObject to, GameObject moved)
    {
        DropZone dropee = moved.GetComponent<DropZone>();
        if (dropee)
        {
            if (dropee.zoneMode == DropZoneMode.Deck)
            {
                if (column[dropee.colIdx].Last != null)
                {
                    column[dropee.colIdx].RemoveLast();
                    if (column[dropee.colIdx].Last != null)
                    {
                        column[dropee.colIdx].Last.Value.GetDraggable().enabled = true;
		            }
                }
            }
        }

        DropZone dropzone = to.GetComponent<DropZone>();
        if (dropzone)
        {
            if (dropzone.zoneMode == DropZoneMode.Deck)
            {
                dropee.colIdx = dropzone.colIdx;
                dropee.GetCard().GetDraggable().enabled = true;
                dropee.zoneMode = DropZoneMode.Deck;
                if (column[dropzone.colIdx].Last != null)
                {
                    column[dropzone.colIdx].Last.Value.GetDraggable().enabled = false;
                    column[dropzone.colIdx].AddLast(dropee.GetCard());
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
                DropZone pile = card.GetDropZone();
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
            card[i].SetValue(i);
        }
        
        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            seed = (seed * 214013 + 2531011) & 0xffffffff;
            c = ((seed >> 16) & 0x7fff) % cardsleft;

            Card addee = card[cards[c]];
            int colIdx = i % 8;
            column[colIdx].AddLast(addee);
            addee.GetDropZone().colIdx = colIdx;

            cards[c] = cards[cardsleft - 1];
            cardsleft -= 1;
        }
    }

    void ChangeCardFront()
    {
        cardidx += 1;
        cardidx %= cardFronts.Length;

        if (resourceHandle.IsValid())
        {
           Addressables.Release(resourceHandle);
	    }
        
        cardFronts[cardidx].LoadAssetAsync<Sprite[]>().Completed += (obj) =>
	    {
            resourceHandle = obj;
            sprites = obj.Result;
            ContinueDealing();
        };
    }

}
