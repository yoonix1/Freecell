using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public UnityEngine.UI.Image img;
    public Deck theDeck;
    public Suit suit ;
    public int value;

    public void SetValue(int mixedValue)
    {
        img.sprite = theDeck.GetSprite(mixedValue);
        value = mixedValue % 13 + 1;
        suit = (Suit)(mixedValue / 13);
        
        name = suit.ToString() + " " + value.ToString() + " " +  mixedValue.ToString();
    }
}
