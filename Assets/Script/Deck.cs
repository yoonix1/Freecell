using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour
{
    public UnityEngine.UI.Button deal;
    void Start()
    {
        deal.onClick.AddListener(onDeal);
    }

    private void onDeal()
    {
        Debug.Log("Deal");
        this.gameObject.SetActive(false);
    }

}
