using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public UnityEngine.UI.Image img;
    public Suit suit ;
    public int value;

    private DropZone dropZone;
    private Draggable draggable;
    private RectTransform rect;

    public DropZone GetDropZone() { return dropZone;  }
    public Draggable GetDraggable() { return draggable;  }
    public RectTransform GetRect() { return rect;  }

    public Deck theDeck
    {
        get => dropZone.theDeck;
        set 
    	{
            dropZone.theDeck = value;
	    }
    }

    private string[] suitToString = new[] {"H", "S", "D", "C"};

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        dropZone = GetComponent<DropZone>();
        draggable = GetComponent<Draggable>();
    }

    public void SetValue(int mixedValue)
    {
        theDeck.GetSprite(mixedValue);
        img.sprite = theDeck.GetSprite(mixedValue);
        value = mixedValue % 13 + 1;
        suit = (Suit)(mixedValue / 13);
        
        name = suitToString[(int)suit] + value.ToString();
    }
}
