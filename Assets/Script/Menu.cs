using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Menu : MonoBehaviour
{
    private Deck theDeck;
    Deck.Callback _callback;

    public void Show(Deck.Callback c)
    {
        _callback = c;
        gameObject.SetActive(true);
        GetComponent<RectTransform>().localScale = new Vector3(1.0f, 0.0f, 0.2f);
        LeanTween.scaleY(gameObject, 1.0f, 0.5f).setEaseInOutCubic();
    }

    public void QuitGame()
    {
        Boolean param = true;
        LeanTween.scaleY(gameObject, 0.1f, 0.2f).setEaseInOutCubic().setOnComplete(_quitAction).setOnCompleteParam(param);
    }

    public void Cancel()
    {
        Boolean param = false;
        LeanTween.scaleY(gameObject, 0.1f, 0.2f).setEaseInOutCubic().setOnComplete(_quitAction).setOnCompleteParam(param);
    }

    void _quitAction(object success)
    {
        Boolean s = (Boolean) success;
        gameObject.SetActive(false);

        if (s == true)
	    { 
            _callback();
	    }
    }
}
