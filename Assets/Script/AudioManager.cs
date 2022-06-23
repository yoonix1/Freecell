using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SoundEffect
{ 
    CardDropped = 0,
}

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    private AudioSource[] sources;

    public static AudioManager Instance;

    private void Awake()
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

    public void Play(SoundEffect se)
    {
        int val = (int)se;
	    if (sources[val] != null)
        {
            sources[val].Play();
	    }
    }

}
