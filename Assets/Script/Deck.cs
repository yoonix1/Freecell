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
    public static Deck Instance { get; private set; }

    public Button deal;
    public Button options;
    public Button undo;
    public Menu quitMenu;
    public Menu selectSkinMenu;

    public Pile objPile;
    public CardFront objCard;

    public Text dispTime;
    public Text dispScore;

    public CardFront[] GetCards() { return card; }

    public delegate void Callback();

    private Boolean isPlaying = false;
    private int score = 0;
    private DateTime lastFinishedTime;

    private CardFront[] card = new CardFront[Constants.NUMBER_OF_CARDS];

    private Pile[] pile = new Pile[4];
    private Pile[] work = new Pile[4];
    private Pile[] colPile = new Pile[8];


    private float width = 0;
    private float height = 0;
    private int cardStyle = 0;
    private float playTime;

    private System.Random rand = new System.Random();

    private bool _isCardMoving;
    public bool IsCardMoving
    {
        get => _isCardMoving;
        set { _isCardMoving = value; if (!value) UpdateDeck(); }
    }


    class CardMove
    {
        public CardMove(Card c, DropZone t) { card = c; from = c.GetDropZone(); to = t; }
        public Card card;
        public DropZone from;
        public DropZone to;
    }

    private LinkedList<CardMove> moveHistory = new LinkedList<CardMove>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;

            quitMenu.MenuHandler += handleQuit;
            selectSkinMenu.MenuHandler += handleSelectSkin;
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
            work[i].name = "WrK" + i;
            work[i].SetColor(color2);
            work[i].dropZone.colIdx = i;
            work[i].dropZone.zoneMode = DropZoneMode.Work;
            work[i].dropZone.name = "WRK" + i;
        }
        for (i = 0; i < 4; i++)
        {
            pile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            pile[i].name = "PiL" + i;
            pile[i].SetColor(color1);
            pile[i].dropZone.colIdx = i;
            pile[i].dropZone.zoneMode = DropZoneMode.Pile;
            pile[i].dropZone.name = "PIL" + i;
        }

        for (i = 0; i < 8; i++)
        {
            colPile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            colPile[i].name = "CoL" + i;
            colPile[i].SetColor(color2);
            colPile[i].dropZone.GetRect().sizeDelta = new Vector3(Constants.CARD_WIDTH + Constants.CARD_PADDING, height);
            colPile[i].dropZone.zoneMode = DropZoneMode.Deck;
            colPile[i].dropZone.colIdx = i;
            colPile[i].dropZone.name = "COL" + i;
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

    private void OnDestroy()
    {
        quitMenu.MenuHandler -= handleQuit;
        selectSkinMenu.MenuHandler -= handleSelectSkin;
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
        ChangeCardFront(cardStyle);
    }

    private void InitGame()
    {
        uint seed = (uint)rand.Next(0, 214013);
        DoDeal(seed);
        DrawDeck();
        MarkAllCardsOnDeck();
        score = 0;
        dispScore.text = score.ToString();
    }

    private void StartGame()
    {
        playTime = 0;
        score = 0;
        lastFinishedTime = DateTime.Now;
        isPlaying = true;
        dispScore.text = score.ToString();
        moveHistory.Clear();
        UpdateDeck();
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

        for (icol = 0; icol < 8; icol++)
        {
            itr[icol] = colPile[icol].dropZone.GetStack().First;
        }

        while (cardCount < Constants.NUMBER_OF_CARDS)
        {
            offset.y = -(Constants.CARD_GAP * 2) * irow + Constants.SCREEN_CARDS_OFFSET_H;
            for (icol = 0; icol < 8; icol++)
            {
                Card c = itr[icol]?.Value;
                if (c != null)
                {
                    offset.x = icol * (Constants.CARD_GAP + Constants.CARD_WIDTH) + Constants.SCREEN_OFFSET_W;
                    LeanTween.moveLocal(c.gameObject, offset, 0.25f).setEaseInOutCubic().setOnComplete(_cardSound);
                    c.GetRect().SetAsLastSibling();
                    yield return new WaitForSeconds(0.1f);
                }
                itr[icol] = itr[icol]?.Next;
                cardCount++;
            }
            irow++;
        }

        StartGame();
    }

    private void _cardSound()
    {
        AudioManager.Instance.Play(SoundEffect.CardDropped);
    }

    public void OnCardMoved(DropZone from, DropZone to)
    {
        _cardSound();
        if (to != null && to.zoneMode == DropZoneMode.Pile)
        {
            KeepTrackScore();
        }

    }

    public void UpdateDeck()
    {
        int availableSlots = 0;
        foreach (Pile w in work)
        {
            if (w.dropZone.GetStack().Last == null)
            {
                availableSlots++;
            }
        }

        // allow stackable moves
        for (int i = 0; i < 8; i++)
        {
            LinkedListNode<Card> prev = colPile[i].dropZone.GetStack().Last;
            LinkedListNode<Card> prevNext = null;
            int count = 0;
            bool draggable = true;

            while (prev != null)
            {
                if (draggable)
                {
                    if (count == availableSlots + 1)
                    {
                        draggable = false;
                    }
                    else if (prevNext != null)
                    {
                        CardFront prevCard = (CardFront)prevNext.Value;
                        CardFront currentCard = (CardFront)prev.Value;

                        if (!(
                            (prevCard.value + 1 == currentCard.value)
                            && ((prevCard.IsBlack() && currentCard.IsRed()) || (currentCard.IsBlack() && prevCard.IsRed()))
                            ))
                        {
                            draggable = false;
                        }
                    }
                }
                prev.Value.enabled = draggable;
                prevNext = prev;
                prev = prev.Previous;
                count++;
            }
        }

        TryAutoMove();
    }

    private void TryAutoMove()
    {
        if (IsCardMoving)
            return;

        Card found = null;
        Pile dest = null;

        int minCardValue = 13;
        foreach (Pile p in pile)
        {
            CardFront c = (CardFront)p.dropZone.GetStack().Last?.Value;
            if (c == null)
            {
                minCardValue = 0;
            }
            else if (c.value < minCardValue)
            {
                minCardValue = c.value;
            }
        }

        FindAutoMoveCandidate(work, minCardValue, out found, out dest);
        if (!found)
        {
            FindAutoMoveCandidate(colPile, minCardValue, out found, out dest);
        }

        if (found)
        {
            AutoMove(dest.dropZone, found);
        }
    }


    private void FindAutoMoveCandidate(Pile[] source, int minCardValue, out Card found, out Pile destination)
    {
        destination = null;
        found = null;
        int i = 0;
        while (!found && i < source.Length)
        {
            CardFront temp = (CardFront)source[i].dropZone.GetStack().Last?.Value;
            int j = 0;
            while (!found && temp && j < 4)
            {
                if (pile[j].dropZone.IsLegalMove(temp) && (temp.value - 2 <= minCardValue))
                {
                    found = temp;
                    destination = pile[j];
                }
                j++;
            }
            i++;
        }
    }


    private void AutoMove(DropZone p, Card c)
    {
        IsCardMoving = true;
        DropZone movingCardDropZone = c.GetDropZone();

        if (p.GetStack().Last != null)
        {
            p.GetStack().Last.Value.enabled = false; // disable picking up when there is animation for it.
        }

        c.GetRect().SetAsLastSibling();

        Vector3 dest = p.GetAnchorPos();
        if (p.zoneMode == DropZoneMode.Deck)
        {
            dest = new Vector3(dest.x, dest.y - p.GetStack().Count * Constants.PILE_OFFSET, dest.z);
        }
        p.MoveTo(c);

        LeanTween.moveLocal(c.gameObject, dest, 0.3f).setEaseInOutCubic().setOnComplete(_AutoDropCompleted).setOnCompleteParam(c);
    }

    private void _AutoDropCompleted(object obj)
    {
        Card c = (Card)obj;

        c.enabled = true;

        IsCardMoving = false;
        UpdateDeck();
        _cardSound();
    }

    public void KeepTrackScore()
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
        foreach (Card c in card)
        {
            c.GetRect().anchoredPosition = new Vector2(0, height / 2 + Constants.CARD_HEIGHT);
        }

        foreach (Pile z in pile)
        {
            z.dropZone.enabled = true;
        }
    }

    private void MarkAllCardsOnDeck()
    {
        // Get The Deck Ready to Play
        foreach (Pile p in pile)
        {
            p.dropZone.GetStack().Clear();
        }
        foreach (Pile p in work)
        {
            p.dropZone.GetStack().Clear();
        }
    }

    private void DoDeal(uint seed)
    {
        int i;
        uint c;
        uint cardsleft = Constants.NUMBER_OF_CARDS;

        foreach (Pile col in colPile)
        {
            col.dropZone.GetStack().Clear();
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
            colPile[colIdx].dropZone.Add(addee);

            cards[c] = cards[cardsleft - 1];
            cardsleft -= 1;
        }
    }

    bool YouWon()
    {
        int piledCards = 0;
        foreach (Pile dz in pile)
        {
            piledCards += dz.dropZone.GetStack().Count;
        }
        if (piledCards == Constants.NUMBER_OF_CARDS)
        {
            return true;
        }
        return false;
    }

    public void ShowMenu()
    {
        quitMenu.Show();
    }

    public void ShowSelectStyle()
    {
        selectSkinMenu.Show();
    }

    void handleQuit(object sender, EventArgs m)
    {
        MenuCloseMsg msg = (MenuCloseMsg)m;
        if (msg.selection == 1) 
	    {
            _stopGame();
	    }
    }

    void _stopGame ()
    { 
        isPlaying = false;
        deal.gameObject.SetActive(true);
    }

    void handleSelectSkin(object sender, EventArgs m)
    {
        MenuCloseMsg msg = (MenuCloseMsg)m;
        ChangeCardFront(msg.selection);
    }

    void ChangeCardFront(int idx)
    {
        cardStyle = idx;
	    AssetManager.Instance.LoadCardFront(idx);
    }

    void OnCardFrontChanged(object dispatcher, EventArgs evt )
	{
        if (isPlaying == false)
        {
            InitGame();
        }
        else 
	    { 
            foreach(CardFront c in card)
            {
                c.Refresh();
	        }
	    }
	}

    public void OnUndo()
    {
        if (moveHistory.Last != null)
        {
            CardMove move = moveHistory.Last.Value;
            moveHistory.RemoveLast();
            LinkedListNode<Card> node = move.to.GetStack().Last;

            while (node != null && node.Value != move.card)
            {
                node = node.Previous;
	        }

            while ( node != null )
            { 
                AutoMove(move.from, node.Value);
                node = node.Next;
            }
        }
    }

    int FindFreeWorkPile(Pile[] work)
    {
        int i = 0;
        while(work[i].dropZone.GetStack().Count != 0 && i < work.Length)
        {
            i++;
	    }
        return i < work.Length ? i : -1 ;
    }

    public void RecordHistory(LinkedList<Card> list, DropZone p)
    {
        LinkedListNode<Card> node = list.First;

        if (node == null)
            return;

        DropZone current = node.Value.GetDropZone();
        if (p == current)
            return;

        moveHistory.AddLast(new CardMove(node.Value, p));
    }

}
