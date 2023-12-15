using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

public class ConventionMode : MonoBehaviour
{ 
    private bool conventionModeEnabled = false;
    public GameObject videoPanel;
    public VideoPlayer videoPlayer;
    private Coroutine c;

    public void OnConventionMode(InputValue value)
    {
        if(!value.isPressed)
            return;
        ToggleConventionMode();
    }

    private void ToggleConventionMode()
    {
        if(conventionModeEnabled)
            DisableConventionMode();
        else
            EnableConventionMode();
    }

    private void EnableConventionMode()
    {
        conventionModeEnabled = true;
        c = StartCoroutine(PrepareVideoPlayer());
    }

    private IEnumerator PrepareVideoPlayer()
    {
        print("preparing");
        if(videoPlayer != null && videoPlayer.enabled) 
        {
            videoPlayer.Prepare();
            while(!videoPlayer.isPrepared)
            {
                yield return new WaitForEndOfFrame();
            }
            videoPlayer.frame = 0;
            videoPlayer.Pause();
            yield return new WaitForEndOfFrame();
        }
        videoPanel.SetActive(true);
        videoPlayer.Play();
        AudioManager.Instance.StopMusic();
        Cursor.visible = false;
    }

    private void DisableConventionMode()
    {
        StopAllCoroutines();
        conventionModeEnabled = false;
        videoPanel.SetActive(false);
        videoPlayer.Stop();
        AudioManager.Instance.PlayMusic();
        Cursor.visible = true;
    }
}
