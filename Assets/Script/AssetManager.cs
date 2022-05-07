using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;


public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    public event EventHandler OnAssetLoaded;
     
    [SerializeField]
    private AssetReference[] cardFronts;

    [SerializeField]
    private AudioSource[] audioSources;


    private Sprite[] sprites = new Sprite[Constants.NUMBER_OF_CARDS];
    private AsyncOperationHandle<Sprite[]> cardFrontHandle;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
	    }
        else
        {
            Instance = this;
	    }
    }

    public void LoadCardFront(int cardidx)
    {
        if (cardFrontHandle.IsValid())
        {
            Addressables.Release(cardFrontHandle);
        }

        cardFronts[cardidx].LoadAssetAsync<Sprite[]>().Completed += (obj) =>
        {
            cardFrontHandle = obj;
            sprites = obj.Result;
	        OnAssetLoaded?.Invoke(this, EventArgs.Empty);
        };
    }

    public Sprite GetSprite(int i) { return sprites[i]; }
}
