using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AudioClip[] songs;
    public float[] songVolume;

    public bool autoPlay = false;
    public int autoOne = 0;
    public int autoTwo = 1;

    public AudioSource[] audioPlayers;
    [HideInInspector]
    public int[] currentSong;
    private int[] nextSong;
    private float[] transitionProgress;
    private float[] transitionTime;
    private float[] oldVolume;

    void Awake()
    {
        currentSong = new int[audioPlayers.Length];
        for (int i = 0; i < currentSong.Length; ++i)
            currentSong[i] = -1;
        nextSong = new int[audioPlayers.Length];
        for (int i = 0; i < nextSong.Length; ++i)
            nextSong[i] = -1;
        transitionProgress = new float[audioPlayers.Length];
        transitionTime = new float[audioPlayers.Length];
        oldVolume = new float[audioPlayers.Length];
        if (autoPlay)
        {
            TransitionTo(0, autoOne, 1f, autoTwo);
        }
    }

    public void TransitionTo(int layerID, int songID, float newTransTime = 0f, int nextSongID = -1)
    {
        //Debug.Log("(" + layerID + ") Transition " + currentSong[layerID] + " into " + songID);
        //Debug.Log(audioPlayers[0] == audioPlayers[1]);
        if (currentSong[layerID] == songID)
            return;
        nextSong[layerID] = songID;
        transitionProgress[layerID] = 0f; // fix this to work with half transitioned shit
        oldVolume[layerID] = audioPlayers[layerID].volume;
        transitionTime[layerID] = newTransTime;
        if (nextSongID >= 0)
            StartCoroutine(WaitTransitionTo(layerID, nextSongID, songs[songID].length));
    }

    IEnumerator WaitTransitionTo(int layerID, int songID, float waitTime)
    {
        //Debug.Log("waiting for " + waitTime);
        yield return new WaitForSeconds(waitTime);
        //Debug.Log("now playing waited (" + layerID + ")");
        TransitionTo(layerID, songID);
    }

    void Update()
    {
        for (int i = 0; i < audioPlayers.Length; ++i)
        {
            if (transitionProgress[i] <= transitionTime[i])
            {
                float halfTotalTime = transitionTime[i] * 0.5f;
                if (transitionProgress[i] >= halfTotalTime)
                {
                    //Debug.Log("song " + currentSong[i] + " to " + nextSong[i]);
                    if (currentSong[i] != nextSong[i])
                    {
                        currentSong[i] = nextSong[i];
                        //Debug.Log("song " + currentSong[i] + " to " + nextSong[i]);
                        if (currentSong[i] >= 0)
                        {
                            audioPlayers[i].clip = songs[currentSong[i]];
                            audioPlayers[i].Play();
                        }
                        else
                        {
                            audioPlayers[i].Stop();
                        }
                        if (i != 0)
                            audioPlayers[i].time = audioPlayers[0].time;
                        else
                        {
                            for (int z = 1; z < audioPlayers.Length; ++z)
                            {
                                audioPlayers[z].time = audioPlayers[0].time;
                                if (currentSong[i] < 0)
                                {
                                    audioPlayers[z].Stop();
                                }
                            }
                        }
                    }
                    else if (currentSong[i] >= 0 && audioPlayers[i].volume >= songVolume[currentSong[i]] || currentSong[i] < 0 && audioPlayers[i].volume >= 1f)
                        break;
                    if (currentSong[i] >= 0 && transitionProgress[i] >= transitionTime[i])
                        audioPlayers[i].volume = songVolume[currentSong[i]];
                    else if (currentSong[i] < 0 && transitionProgress[i] >= transitionTime[i])
                        audioPlayers[i].volume = 1f;
                    else if (currentSong[i] >= 0 && transitionProgress[i] < transitionTime[i])
                        audioPlayers[i].volume = Mathf.SmoothStep(0f, songVolume[currentSong[i]], (transitionProgress[i] - halfTotalTime) / halfTotalTime);
                    else if (currentSong[i] < 0 && transitionProgress[i] < transitionTime[i])
                        audioPlayers[i].volume = Mathf.SmoothStep(0f, 1f, (transitionProgress[i] - halfTotalTime) / halfTotalTime);
                    transitionProgress[i] += Time.deltaTime;
                }
                else
                {
                    audioPlayers[i].volume = Mathf.SmoothStep(oldVolume[i], 0f, transitionProgress[i] / halfTotalTime);
                    transitionProgress[i] += Time.deltaTime;
                }
            }
        }
    }
}
