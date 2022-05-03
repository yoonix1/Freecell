using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Debug = UnityEngine.Debug;

using UnityEditor;

public class Deck : MonoBehaviour
{
    public Button deal;
    public Button options;

    public DropZone objPile;
    public Card objCard;

    public Text dispTime;
    public Text dispScore;

    private Boolean isPlaying = false;
    private int score = 0;
    private DateTime lastFinishedTime;

    [SerializeField]
    private AssetReference[] cardFronts;

    [SerializeField]
    private AudioSource[] audioSources;

    private Card[] card = new Card[Constants.NUMBER_OF_CARDS];

    private LinkedList<Card>[] column = new LinkedList<Card>[8];
    private DropZone[] pile = new DropZone[4];
    private DropZone[] work = new DropZone[4];
    private DropZone[] colPile = new DropZone[8];

    private Sprite[] sprites = new Sprite[Constants.NUMBER_OF_CARDS];

    private AsyncOperationHandle resourceHandle;

    private int cardidx = 0;
    private float width = 0;
    private float height = 0;
    private float playTime;

    private float numAvailableSlots = 4;
    private System.Random rand = new System.Random();

    private void Awake()
    {
        int i;
        RectTransform rect = GetComponent<RectTransform>();
        width = rect.rect.width;
        height = rect.rect.height;

        Color color1 = new Color32(32,62,72,98);
        Color color2 = new Color32(174,161,82,76);

        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i] = Instantiate(objCard, Vector3.zero, Quaternion.identity, rect);
            card[i].theDeck = this;
        }

        for (i = 0; i < 4; i++)
        { 
            work[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            work[i].SetColor(color2);
            work[i].theDeck = this;
        }
        for (i = 0; i < 4; i++)
        {
            pile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            pile[i].theDeck = this;
            pile[i].SetColor(color1);
        }

        for (i = 0; i < 8; i++)
        {
            colPile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            colPile[i].SetColor(color2);
            colPile[i].theDeck = this;
            column[i] = new LinkedList<Card>();
        }
    }

    void Start()
    {
        int i;

        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i].SetValue(i);
        }


        float colwidth = Constants.PILE_GAP + Constants.CARD_WIDTH;
        for (i = 0; i < 4; i++)
        {
            pile[i].GetRect().anchoredPosition = new Vector2(colwidth * i + Constants.SCREEN_OFFSET_PILE, Constants.SCREEN_OFFSET_H);
            pile[i].zoneMode = DropZoneMode.Pile;
            pile[i].colIdx = i;

            work[i].GetRect().anchoredPosition = new Vector2(colwidth * i + Constants.SCREEN_OFFSET_W, Constants.SCREEN_OFFSET_H);
            work[i].zoneMode = DropZoneMode.Work;
            work[i].colIdx = i;
        }

        colwidth = Constants.CARD_GAP + Constants.CARD_WIDTH;
        for (i = 0; i < 8; i++)
        {
            colPile[i].GetRect().anchoredPosition = new Vector2(colwidth * i + Constants.SCREEN_OFFSET_W, Constants.SCREEN_CARDS_OFFSET_H);
            colPile[i].zoneMode = DropZoneMode.Deck;
            colPile[i].colIdx = i;
        }
    }

    public void Update()
    {
        if (isPlaying)
        {
            playTime += Time.deltaTime;

            int seconds = (int)playTime;

            String myTime;

            myTime = String.Format("{0}:{1,2:D2}", seconds / 60, seconds % 60);
            dispTime.text = myTime;

            if (YouWon())
            {
                isPlaying = false;
                deal.gameObject.SetActive(true);
            }
        }
    }

    public void OnDeal()
    {
        deal.gameObject.SetActive(false);
        //deal.gameObject.SetActive(false);
        ChangeCardFront();
        // after the card texture is loaded this function will call ContinueDealing
    }

    public LinkedList<Card> GetColumnAfter(Card c)
    {
        int idx = c.GetDropZone().colIdx;
        LinkedList<Card> result = new LinkedList<Card>();
        LinkedListNode<Card> toMove = column[idx].Find(c);


        while (toMove.Next != null)
        {
            LinkedListNode<Card> item = toMove.Next;
            column[idx].Remove(item);
            result.AddLast(item);
	    }

        return result;
    }

    public void PlaySound(SoundEffect se)
    {
        switch (se)
        {
            case SoundEffect.CardDropped:
                audioSources[(int)se].Play();
                break;
            default:
                break;
        }
    }


    private void ContinueDealing()
    {
        uint seed = (uint)rand.Next(0, 214013);
        DoDeal(seed);
        DrawDeck();
        MarkAllCardsOnDeck();
        playTime = 0;
        score = 0;
        lastFinishedTime = DateTime.Now;
        isPlaying = true;
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
            offset.x = icol * (Constants.CARD_GAP + Constants.CARD_WIDTH) + Constants.SCREEN_OFFSET_W;
            i = 0;
            foreach (Card item in cards)
            {
                offset.y = -(Constants.CARD_GAP*2) * i + Constants.SCREEN_CARDS_OFFSET_H;
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

    public bool CanBeDropped(DropZoneMode zoneMode, Card dropzoneCard, Card dropee)
    {
        DropZone dropeePile = dropee.GetComponent<DropZone>();
        if (dropeePile.zoneMode == DropZoneMode.Pile)
        {
            return false;
        }
        else if (zoneMode == DropZoneMode.Deck || zoneMode == DropZoneMode.Sorted)
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
            if (dropzoneCard == null)
            {
                return !dropee.GetDraggable().IsStackMove();
            }
            return false;
        }
        else if (zoneMode == DropZoneMode.Pile)
        {
            if (dropzoneCard == null)
            {
                if (dropee.value == 1)
                {
                    return !dropee.GetDraggable().IsStackMove();
                }
            }
            else
            {
                if (dropzoneCard.value + 1 == dropee.value && dropzoneCard.suit == dropee.suit)
                {
                    return !dropee.GetDraggable().IsStackMove();
                }
            }
            return false;
        }
        else
        {
            return false;
        }
    }

    public void MoveStackTo(Card from, LinkedList<Card> stack)
    {
        RectTransform orig = from.GetRect();
        int colIdx = from.GetDropZone().colIdx;

        if (stack != null)
        {
            int count = 1;
            while (stack.First != null)
            {
                LinkedListNode<Card> toRemove = stack.First;
                stack.RemoveFirst();
                column[colIdx].AddLast(toRemove);

                Card card = toRemove.Value;
                RectTransform rect = card.GetRect();
                Draggable draggable = card.GetDraggable();
                rect.SetAsLastSibling();
                rect.anchoredPosition = new Vector2(orig.anchoredPosition.x, orig.anchoredPosition.y - count*Constants.PILE_OFFSET);
                draggable.Dropped(colIdx);

                count++;
            }
        }
    }

    public void UpdateDropZones(DropZone dropzone, Draggable moved)
    {
        DropZone dropee = moved.GetComponent<DropZone>();
        if (dropee)
        {
            if (dropee.IsOnDeck())
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

            if (dropee.zoneMode == DropZoneMode.Work)
            {
                numAvailableSlots++;
	        }
        }

        if (dropzone)
        {
            if (dropzone.IsOnDeck())
            {
                dropee.colIdx = dropzone.colIdx;
                dropee.zoneMode = DropZoneMode.Sorted;
                if (dropzone.GetCard() != null)
                {
                    dropzone.zoneMode = DropZoneMode.Sorted;
                }

                Card movingCard = dropee.GetCard();
                column[dropzone.colIdx].AddLast(movingCard);
                movingCard.GetRect().SetAsLastSibling();

                MoveStackTo(movingCard, movingCard.GetDraggable().GetStack());
            }
            else if (dropzone.zoneMode == DropZoneMode.Pile)
            {
                if (!dropee.GetCard().GetDraggable().IsStackMove())
                {
                    dropzone.enabled = false;
                    dropee.enabled = true;
                    //dropee.GetCard().GetDraggable().enabled = false;
                    //dropee.GetComponent<CanvasGroup>().blocksRaycasts = false;
                    dropee.zoneMode = DropZoneMode.Pile;


                    DateTime now = DateTime.Now;
                    TimeSpan delta = now.Subtract(lastFinishedTime);
                    double val = 30 / delta.Seconds;
                    score += (int)Math.Ceiling(val);
                    lastFinishedTime = now;
                }
            }
            else if (dropzone.zoneMode == DropZoneMode.Work)
            {
                if (!dropee.GetCard().GetDraggable().IsStackMove())
                { 
                    dropee.zoneMode = DropZoneMode.Work;
                    numAvailableSlots--;
                }
            }
        }

        dispScore.text = score.ToString();

        for( int i = 0; i < 8; i++)
        {
            int j = 0;
            int totalCardsPerColumn = column[i].Count;
            foreach(Card c in column[i])
            {
                if (j + numAvailableSlots >= totalCardsPerColumn - 1 && c.GetDropZone().zoneMode == DropZoneMode.Sorted) 
		        {
                    c.GetDraggable().enabled = true;
		        }
                else if (j == totalCardsPerColumn - 1) 
		        {
                    c.GetDraggable().enabled = true; // last card is always active
		        }
                else
                { 
                    c.GetDraggable().enabled = false;
		        }
                j++;
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
            col.Last.Value.GetDropZone().zoneMode = DropZoneMode.Sorted;

            idx++;
        }
    }

    private void DoDeal(uint seed)
    {
        int i;
        uint c;
        uint cardsleft = Constants.NUMBER_OF_CARDS;

        foreach (LinkedList<Card> col in column)
        {
            col.Clear();
        }

        int[] cards = new int[Constants.NUMBER_OF_CARDS];
        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            cards[i] = i;
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

    bool YouWon()
    {
        int totalCardsOnDeck = 0;
        foreach(LinkedList<Card> c in column)
        {
            totalCardsOnDeck += c.Count;
	    }
        if (numAvailableSlots == 4 && totalCardsOnDeck == 0)
        {
            return true;
	    }
        return false;
    }

    //static int debugcount = 0;
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
            //Debug.Log("Loaded cards" + debugcount); debugcount = debugcount +1
        };
    }
}
