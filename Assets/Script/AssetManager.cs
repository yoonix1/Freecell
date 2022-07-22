using System;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class AssetManager : MonoBehaviour
{
    public static AssetManager Instance { get; private set; }

    public event EventHandler OnAssetLoaded;

    //private String[] resourceNames = { "bundle1", "bundle2", "bundle3", "bundle4" };
    private String[] resourceNames = { "mycards1", "mycards2", "mycards3", "mycards4" }; // Addressable
    public AssetReference[] objects;
    [SerializeField]
    private AudioSource[] audioSources;

    [SerializeField]
    private String hostbase = "http://192.168.7.65/";
    private String host = "http://192.168.7.65/StandaloneOSX/"; // this is reset on Awake depends on the plantform

    private Sprite[] sprites = new Sprite[Constants.NUMBER_OF_CARDS];

    private AsyncOperationHandle<Sprite[]> cardFrontHandle;
    private AssetBundle bundle;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
	    }
        else
        {
            Instance = this;
            Addressables.InitializeAsync();
            // m_handle = Addressables.LoadResourceLocationsAsync("mycards1");
        }

        String platform = "StandaloneOSX";
#if UNITY_ANDROID
        platform = "Android";
#endif
        host = hostbase + platform + "/";

    }

    public void LoadCardFront(int cardidx)
    {
        //StartCoroutine("LoadFromBundle", resourceNames[cardidx]);
        if (cardFrontHandle.IsValid())
        {
            Addressables.Release(cardFrontHandle);
        }

        //   foreach( var locator in  Addressables.ResourceLocators)
        //   {
        //       foreach(var item in locator.Keys )
        //       { 
        //           Debug.Log(String.Format("{0} {1} ", item.ToString(), item.GetType()));
        //       }
        //   }

        Addressables.LoadAssetAsync<Sprite[]>(resourceNames[cardidx]).Completed += obj =>
            {
                cardFrontHandle = obj;
                sprites = obj.Result;
                OnAssetLoaded?.Invoke(this, EventArgs.Empty);
            };
    }

    IEnumerator LoadFromBundle(String target)
    {
        Caching.ClearCache();
        using (var uwr = UnityWebRequestAssetBundle.GetAssetBundle(host + target))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // free previously loaded objects
                if (bundle != null)
                {
                    bundle.Unload(true);
		        }
                bundle = DownloadHandlerAssetBundle.GetContent(uwr);
                sprites = bundle.LoadAllAssets<Sprite>();
                OnAssetLoaded?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public Sprite GetSprite(int i) { return sprites[i]; }
}
