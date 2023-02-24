using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;

public class CutsceneManager : MonoBehaviour
{
    public List<CutsceneCaption> captions;
    public TMPTextTyper typer;
    public VideoPlayer videoPlayer;

    private float time = 0;
    private bool active = false;
    private bool paused = false;


    private float mintime = 17f;


    private void Start() {
        StartCutscene();
    }

    private void Update() {
        if(active && !paused) {
            time += Time.deltaTime;
            foreach(CutsceneCaption c in captions)
            {
                if(time > c.startTime && !c.played)
                {
                    StartCoroutine(ShowCaption(c));
                    c.played = true;
                }
            }

        } 

        if(time > mintime && active && !videoPlayer.isPlaying && !videoPlayer.isPaused)
            EndCutscene();
    }
    

    public void StartCutscene()
    {
        videoPlayer.Play();
        active = true;

    }

    private IEnumerator ShowCaption(CutsceneCaption caption)
    {
        typer.StartTyping(caption.text);
        if(caption.pauseOnDisplay) {
            videoPlayer.Pause();
            paused = true;
            yield return new WaitForSeconds(caption.duration);
            videoPlayer.Play();
            paused = false;
        }
    }



    public void EndCutscene()
    {
        LevelManager.Instance.LoadLevel(0);
    }
}

[Serializable]
public class CutsceneCaption
{
    public string text;
    public float startTime;
    public float duration;
    public bool pauseOnDisplay;
    public bool played = false;
}


