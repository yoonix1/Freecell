using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    public UnityEngine.UI.Image img;
    public Deck theDeck;
    public Suit suit ;
    public int value;

    private DropZone dropZone;
    private Draggable draggable;
    private RectTransform rect;

    public DropZone GetDropZone() { return dropZone;  }
    public Draggable GetDraggable() { return draggable;  }
    public RectTransform GetRect() { return rect;  }

    private string[] suitToString = new[] {"H", "S", "D", "C"};

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        dropZone = GetComponent<DropZone>();
        draggable = GetComponent<Draggable>();
    }

    public void SetValue(int mixedValue)
    {
        img.sprite = theDeck.GetSprite(mixedValue);
        value = mixedValue % 13 + 1;
        suit = (Suit)(mixedValue / 13);
        
        name = suitToString[(int)suit] + value.ToString();
    }
}
