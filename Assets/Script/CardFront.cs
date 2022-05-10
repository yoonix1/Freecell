using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardFront : Card
{
    public UnityEngine.UI.Image img;
    public Suit suit ;
    public int value;

    public Card GetCard() { return this;  }

    public bool IsRed() { return suit == Suit.DIAMOND || suit == Suit.HEART; }
    public bool IsBlack() { return suit == Suit.SPADE || suit == Suit.CLUB; }

    private string[] suitToString = new[] {"H", "S", "D", "C"};

    public void SetValue(int mixedValue)
    {
        AssetManager.Instance.GetSprite(mixedValue);
        img.sprite = AssetManager.Instance.GetSprite(mixedValue);
        value = mixedValue % 13 + 1;
        suit = (Suit)(mixedValue / 13);
        
        name = suitToString[(int)suit] + value.ToString();
    }

}
