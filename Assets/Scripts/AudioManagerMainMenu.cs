using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerMainMenu : MonoBehaviour
{
    [SerializeField] AudioClip select, change, join, start, win, lose, humanHurt, buildingHurt, humanDie, buildingDie;
    [SerializeField] AudioSource audio;
    [SerializeField] AudioClip[] Musics;

    public static AudioManagerMainMenu instance;

    private void Awake()
    {
        PlayNextSong();
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    void PlayNextSong()
    {
        AudioClip nowmusic = Musics[Random.Range(0, Musics.Length)];
        GetComponent<AudioSource>().PlayOneShot(nowmusic);
        Invoke("PlayNextSong", nowmusic.length);
    }

    public void PlaySelectSound()
    {
        audio.PlayOneShot(select);
    }
    public void PlayChangeSound()
    {
        audio.PlayOneShot(change);
    }
    public void PlayJoinSound()
    {
        audio.PlayOneShot(join);
    }
    public void PlayStartSound()
    {
        audio.PlayOneShot(start);
    }
    public void PlayWinSound()
    {
        audio.PlayOneShot(win);
    }
    public void PlayLoseSound()
    {
        audio.PlayOneShot(lose);
    }
    public void PlayHHurtSound()
    {
        audio.PlayOneShot(humanHurt);
    }
    public void PlayBHurtSound()
    {
        audio.PlayOneShot(buildingHurt);
    }
    public void PlayHDieSound()
    {
        audio.PlayOneShot(humanDie);
    }
    public void PlayBDieSound()
    {
        audio.PlayOneShot(buildingDie);
    }
}
