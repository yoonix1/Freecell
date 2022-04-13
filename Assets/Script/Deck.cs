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

        for (i = 0; i < 4; i++)
        {
            pile[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect);
            work[i] = Instantiate(objPile, Vector3.zero, Quaternion.identity, rect );
        }
        Resize();
    }


    private void Resize()
    {
        int i;
        float colwidth = width / 8;
        float offset = 50;

        for (i = 0; i < 4; i++)
        {
            work[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(colwidth * i + offset, -100);
            pile[i].GetComponent<RectTransform>().anchoredPosition = new Vector2( colwidth * (i + 4) + offset, -100);
        }
    }

    private void OnDeal()
    {
        //deal.gameObject.SetActive(false);
        DoDeal(10);
        DrawDeck();
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
            offset.x = icol * 75 + 50;
            i = 0;
            foreach (Card item in cards)
            {
                offset.y = -75 * i - 150;
                item.GetComponent<RectTransform>().anchoredPosition = offset;
                item.GetComponent<RectTransform>().SetAsLastSibling();
                i += 1;
            }
            icol += 1;
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
        
        for (i = 0; i < Constants.NUMBER_OF_CARDS; i++)
        {
            seed = (seed * 214013 + 2531011) & 0xffffffff;
            c = ((seed >> 16) & 0x7fff) % cardsleft;
        // remap card number "suit/num" to "num/suit"
        //    uint num = c / 4 + 1;
        //    uint suit = c % 4;
        //    uint mymap = (num - 1) + suit * 13;
            Debug.Log("order" + c.ToString());
            column[i % 8].AddLast(card[c]);
            card[c] = card[cardsleft - 1];
            cardsleft -= 1;
        }
    }
}
