using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private int val;
    
    public UnityEngine.UI.Button me;
    public UnityEngine.UI.Image img;

    public int Value
    {
        get { return val;  }
        set
        {
            if (val != value)
            {
                val = value;
                img.sprite = Resources.Load<Sprite>("Bundles/Card/card_" + val);
            }
        }
    }

    void Start()
    {
        me.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
       Debug.Log("Hello Yoon"); 
       // img.sprite = 
       img.sprite = Resources.Load<Sprite>("Bundles/Card/card_" + val);
       val = val + 1;
    }
}
