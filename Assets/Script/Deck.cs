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
    public static Deck Instance { get; private set;}

    public Button deal;
    public Button options;
    public Menu quitMenu;

    public DropZone objPile;
    public CardFront objCard;

    public Text dispTime;
    public Text dispScore;

    public delegate void Callback();

    private Boolean isPlaying = false;
    private int score = 0;
    private DateTime lastFinishedTime;

    private CardFront[] card = new CardFront[Constants.NUMBER_OF_CARDS];

    private DropZone[] pile = new DropZone[4];
    private DropZone[] work = new DropZone[4];
    private DropZone[] colPile = new DropZone[8];


    private float width = 0;
    private float height = 0;
    private float playTime;

    private System.Random rand = new System.Random();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
	    }


        int i;
        RectTransform rect = GetComponent<RectTransform>();
        width = rect.rect.width;
        height = rect.rect.height;

        Color color1 = new Color32(32, 62, 72, 98);
        Color color2 = new Color32(174, 161, 82, 76);

        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i] = Instantiate(objCard, Vector3.zero, Quaternion.identity, rect);
            card[i].SetValue(i);
        }

        for (i = 0; i < 4; i++)
        {
            work[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            work[i].SetColor(color2);
            work[i].colIdx = i;
            work[i].zoneMode = DropZoneMode.Work;
        }
        for (i = 0; i < 4; i++)
        {
            pile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            pile[i].SetColor(color1);
            pile[i].colIdx = i;
            pile[i].zoneMode = DropZoneMode.Pile;
        }

        for (i = 0; i < 8; i++)
        {
            colPile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            colPile[i].SetColor(color2);
            colPile[i].zoneMode = DropZoneMode.Deck;
            colPile[i].colIdx = i;
        }

	    AssetManager.Instance.OnAssetLoaded += OnCardFrontChanged;
    }

    void Start()
    {
        int i;

        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            card[i].GetRect().anchoredPosition = new Vector2(0, height / 2 + Constants.CARD_HEIGHT);
        }

        float colwidth = Constants.PILE_GAP + Constants.CARD_WIDTH;
        for (i = 0; i < 4; i++)
        {
            pile[i].GetRect().anchoredPosition = new Vector2(colwidth * i + Constants.SCREEN_OFFSET_PILE, Constants.SCREEN_OFFSET_H);
            work[i].GetRect().anchoredPosition = new Vector2(colwidth * i + Constants.SCREEN_OFFSET_W, Constants.SCREEN_OFFSET_H);
        }

        colwidth = Constants.CARD_GAP + Constants.CARD_WIDTH;
        for (i = 0; i < 8; i++)
        {
            colPile[i].GetRect().anchoredPosition = new Vector2(colwidth * i + Constants.SCREEN_OFFSET_W, Constants.SCREEN_CARDS_OFFSET_H);
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
                _stopGame();
            }
        }
    }

    public void OnDeal()
    {
        HideCards();
        deal.gameObject.SetActive(false);
        ChangeCardFront(1);
    }
    /*

    public LinkedList<Card> GetColumnAfter(Card c)
    {
	    LinkedList<Card> currentList = c.GetCurrentDropZone().GetList();
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
    */

    public void PlaySound(SoundEffect se)
    {
        switch (se)
        {
            case SoundEffect.CardDropped:
                break;
            default:
                break;
        }
    }


    private void InitGame()
    {
        uint seed = (uint)rand.Next(0, 214013);
        DoDeal(seed);
        DrawDeck();
        MarkAllCardsOnDeck();
        playTime = 0;
        score = 0;
        lastFinishedTime = DateTime.Now;
        isPlaying = true;
        dispScore.text = score.ToString();

    }

    public void DrawDeck()
    {
        StartCoroutine(_CoroutineDrawDeck());
    }

    private IEnumerator _CoroutineDrawDeck()
    {
        RectTransform myRect = GetComponent<RectTransform>();
        Vector2 offset = Vector2.zero;
        int irow = 0;
        int icol = 0;
	    int cardCount = 0;

	    LinkedListNode<Card>[] itr = new LinkedListNode<Card>[8];

	    for( icol = 0; icol < 8; icol ++) 
	    {
	        itr[icol] = colPile[icol].GetStack().First;
		}

	    while(cardCount < Constants.NUMBER_OF_CARDS) 
	    {
            offset.y = -(Constants.CARD_GAP * 2) * irow + Constants.SCREEN_CARDS_OFFSET_H;
            for( icol = 0; icol < 8; icol++ )
	        {
		        Card c = itr[icol]?.Value;
		        if ( c != null ) 
		        {
                    offset.x = icol * (Constants.CARD_GAP + Constants.CARD_WIDTH) + Constants.SCREEN_OFFSET_W;
                    LeanTween.moveLocal(c.gameObject, offset, 0.25f).setEaseInOutCubic().setOnComplete(_cardSound);
		            c.GetRect().SetAsLastSibling();
                    yield return new WaitForSeconds(0.1f);
		        }
		        itr[icol] = itr[icol]?.Next;
		        cardCount++;
		    }
	        irow ++;
	    }
    }

    private void _cardSound()
    {
        PlaySound(SoundEffect.CardDropped);
    }

    /*

    public bool CanBeDropped(DropZoneMode zoneMode, Card dropzoneCard, Card dropee)
    {
        DropZone dropeePile = dropee.GetComponent<DropZone>();
        if (zoneMode == DropZoneMode.Deck || zoneMode == DropZoneMode.Sorted)
        {
            if (dropeePile.zoneMode == DropZoneMode.Pile)
            {
                return false;
            }
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
                rect.anchoredPosition = new Vector2(orig.anchoredPosition.x, orig.anchoredPosition.y - count * Constants.PILE_OFFSET);
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
            if (dropee.zoneMode == DropZoneMode.Pile)
            {
                pile[dropee.colIdx].enabled = true;
                pile[dropee.colIdx].SetCard(null);
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
                    dropee.colIdx = dropzone.colIdx;

                    pile[dropzone.colIdx].SetCard(dropee.GetCard());
                    KeepTrackScore();

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


        UpdateMovableCards();
    }

    private void UpdateMovableCards()
    {
        for (int i = 0; i < 8; i++)
        {
            int j = 0;
            int totalCardsPerColumn = column[i].Count;
            foreach (Card c in column[i])
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

        TryAutoMove();
    }

    private void TryAutoMove()
    {
        int j = 0;
        int i = 0;
        Card found = null;

        int minCardValue = 13;
        foreach(DropZone p in pile)
        { 
            Card c = p.GetCard();
            if ( c == null )
            {
                minCardValue = 0;
            }
            else if (c.value < minCardValue)
            {
                minCardValue = c.value;
	        }
	    }

        while (found == null && i < 8)
        {
            if (column[i].Last != null)
            {
                Card temp = column[i].Last.Value;

                j = 0;
                while (found == null && j < 4)
                {
                    Card pileCard = pile[j].GetCard();
                    if (pileCard == null)
                    {
                        if (temp.value == 1)
                        {
                            found = temp;
                            break;
                        }
                    }
                    else
                    {
                        if (temp.suit == pileCard.suit && temp.value == pileCard.value + 1 && temp.value <= minCardValue + 2)
                        {
                            found = temp;
                            break;
                        }
                    }
                    j++;
                }
            }
            i++;
	    }

        if (found != null)
        {
            AutoMove(pile[j], found);
	    }
    }


    private void AutoMove(DropZone p, Card c)
    {
        DropZone movingCardDropZone = c.GetDropZone();
        column[movingCardDropZone.colIdx].RemoveLast();
        c.GetRect().SetAsLastSibling();
        p.enabled = false;
        movingCardDropZone.enabled = true;
        movingCardDropZone.zoneMode = DropZoneMode.Pile;
        movingCardDropZone.colIdx = p.colIdx;
        p.SetCard(c);
        LeanTween.moveLocal(c.gameObject, p.GetRect().anchoredPosition3D, 0.3f).setEaseInOutCubic().setOnComplete(_AutoDropCompleted);

        KeepTrackScore();
    }

    private void _AutoDropCompleted()
    {
        UpdateMovableCards();
    }
    */

    private void KeepTrackScore()
    { 
        DateTime now = DateTime.Now;
        TimeSpan delta = now.Subtract(lastFinishedTime);
        double val = 30 / Math.Max(delta.Seconds, 1.0f);
        score += (int)Math.Ceiling(val);
        lastFinishedTime = now;
        dispScore.text = score.ToString();
    }

    private void HideCards()
    { 
        foreach(Card c in card)
        {
            c.GetRect().anchoredPosition = new Vector2(0, height / 2 + Constants.CARD_HEIGHT);
	    }

        foreach(DropZone z in pile)
        {
            z.enabled = true;
	    }
    }

    private void MarkAllCardsOnDeck()
    {
    /*
	    // Get The Deck Ready to Play

        numAvailableSlots = 4;
        foreach (DropZone p in pile)
        {
            p.SetCard(null);
	        p.enabled = true;
        }

        int idx = 0;
        foreach (LinkedList<Card> col in column)
        {
            foreach (Card card in col)
            {
                DropZone pile = card.GetDropZone();
                pile.zoneMode = DropZoneMode.Deck;
                pile.colIdx = idx;
                pile.enabled = true;
            }
            col.Last.Value.GetDropZone().zoneMode = DropZoneMode.Sorted;

            idx++;
        }
    */
    }

    private void DoDeal(uint seed)
    {
        int i;
        uint c;
        uint cardsleft = Constants.NUMBER_OF_CARDS;

        foreach (DropZone col in colPile)
        {
            col.GetStack().Clear();
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
            colPile[colIdx].MoveTo(addee);

            cards[c] = cards[cardsleft - 1];
            cardsleft -= 1;
        }
    }

    bool YouWon()
    {
        int piledCards = 0;
        foreach(DropZone dz in pile)
        {
            piledCards += dz.GetStack().Count;
	    }
        if (piledCards == Constants.NUMBER_OF_CARDS)
        {
            return true;
	    }
        return false;
    }

    public void ShowMenu()
    {
        Callback mycallback = new Callback(this._stopGame);
        quitMenu.Show(mycallback);
    }


    private void _stopGame()
    { 
        isPlaying = false;
        deal.gameObject.SetActive(true);
    }
    

    //static int debugcount = 0;
    void ChangeCardFront(int idx)
    {
	    AssetManager.Instance.LoadCardFront(1);
    }

    void OnCardFrontChanged(object dispatcher, EventArgs evt )
	{
	    InitGame();
	}
}
