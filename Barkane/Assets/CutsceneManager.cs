using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Video;
using UnityEditor.SearchService;
using UnityEngine.SceneManagement;

public class CutsceneManager : MonoBehaviour
{
    public List<CutsceneCaption> captions;
    public TMPTextTyper typer;
    public VideoPlayer videoPlayer;

    public Level NextLevel;
    public int NextSceneIndex = -1;

    private float time = 0;
    private bool active = false;
    private bool paused = false;


    public float mintime = 17f;
    private bool ended = false;

    private bool disabled = false;

    private void Start() {
        if(NextLevel != null)
            LevelManager.Instance.UnlockLevel(NextLevel);
        if(disabled)
        {
            EndCutscene();
            return;
        }        
        Cursor.visible = false;
        StartCoroutine(WaitToStart());
    }

    private IEnumerator WaitToStart()
    {
        videoPlayer?.Play();
        yield return new WaitForEndOfFrame();
        videoPlayer?.Pause();
        yield return new WaitForSeconds(1f);
        StartCutscene();
    }

    private void Update() {
        if(active && !paused) {
            time += Time.deltaTime * (videoPlayer != null ? videoPlayer.playbackSpeed : 1);
            foreach(CutsceneCaption c in captions)
            {
                if(time > c.startTime && !c.played)
                {
                    StartCoroutine(ShowCaption(c));
                    c.played = true;
                    if(c.pauseOnDisplay)
                        StartCoroutine(Pause(c.pauseDuration));
                }
            }

        } 

        if(time > mintime && !ended)
            EndCutscene();
    }
    

    public void StartCutscene()
    {
        videoPlayer?.Play();
        active = true;
    }

    private IEnumerator ShowCaption(CutsceneCaption caption)
    {
        typer.StartTyping(caption.text);
        yield return new WaitForSeconds(caption.duration);
        typer.FadeOutText(1f);
    }

    private IEnumerator Pause(float duration)
    {
        videoPlayer?.Pause();
        paused = true;
        yield return new WaitForSeconds(duration);
        videoPlayer?.Play();
        paused = false;
    }

    public void PauseCutscene(bool pause)
    {
        if(pause)
        {
            videoPlayer?.Pause();
            paused = true;
        }
        else
        {
            videoPlayer?.Play();
            paused = false;
        }
    }

    public void EndCutscene()
    {
        Time.timeScale = 1;
        ended = true;   
        if(NextLevel != null)
            LevelManager.Instance.LoadLevel(NextLevel.levelName);     
        if(NextSceneIndex != -1)
            SceneManager.LoadScene(NextSceneIndex);
        Cursor.visible = true;
    }
}

[Serializable]
public class CutsceneCaption
{
    public string text;
    public float startTime;
    public float duration;
    public float pauseDuration;
    public bool pauseOnDisplay;
    public bool played = false;
}


