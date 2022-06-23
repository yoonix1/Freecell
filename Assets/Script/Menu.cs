using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuCloseMsg : EventArgs {
    public int selection;
}

public class Menu : MonoBehaviour
{
    public event EventHandler MenuHandler;

    public void Show()
    {
        gameObject.SetActive(true);
        GetComponent<RectTransform>().localScale = new Vector3(1.0f, 0.0f, 0.2f);
        LeanTween.scaleY(gameObject, 1.0f, 0.5f).setEaseInOutCubic();
    }

    public void Choose(Int32 selection)
    { 
        LeanTween.scaleY(gameObject, 0.1f, 0.2f).setEaseInOutCubic().setOnComplete(_quitAction).setOnCompleteParam(selection);
    }

    void _quitAction(object selection)
    {
        Int32 s = (Int32) selection;
        gameObject.SetActive(false);

        MenuCloseMsg msg = new MenuCloseMsg();
        msg.selection = s;

        MenuHandler?.Invoke(this, msg);
    }
}
