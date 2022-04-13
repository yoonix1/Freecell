using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Pile : MonoBehaviour, IDropHandler
{
    // Start is called before the first frame update
    LinkedList<Card> list = new LinkedList<Card>();
    public bool onlyShowTop;
    public void AddTop(Card c)
    {
        list.AddFirst(c);
        if (onlyShowTop)
        {
             
        }
    }

    public void RemoveFirst()
    {
        if (list.First != null)
        {
            list.RemoveFirst();
        }
    }

    public Card Top()
    {
        if (list.First == null)
        {
            return null;
        }
        return list.First.Value;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = GetComponent<RectTransform>().anchoredPosition;
        }
    }
}
