using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioSource audioPlayer;
    public AudioClip[] songs;
    private int lastPlayedSong = 0;

    void TransitionTo(int songID, float transitionTime = 1f)
    {
        lastPlayedSong = songID;
    }
}
