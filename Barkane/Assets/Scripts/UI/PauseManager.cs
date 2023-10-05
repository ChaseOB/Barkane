using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class PauseManager : Singleton<PauseManager>
{
    [SerializeField] private GameObject PauseMenu;
    private bool isPaused = false;
    public static bool IsPaused => Instance.isPaused;

    public CutsceneManager cutsceneManager;

    public GameObject options;

    private void Awake() {
        InitializeSingleton();
    }

    public void TogglePause()
    {
        if(!isPaused) Pause();
        else UnPause();
    }

    public void ToggleOptions(bool val)
    {
        PauseMenu.SetActive(!val);
        options.SetActive(val);
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0;
        if(cutsceneManager != null)
        {
            cutsceneManager.PauseCutscene(true);
        }
        PauseMenu.SetActive(true);
       // Cursor.visible = true;
    }

    public void UnPause()
    {
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
        isPaused = false;
        Time.timeScale = 1;
        if(cutsceneManager != null)
        {
            cutsceneManager.PauseCutscene(false);
        }
        PauseMenu.SetActive(false);
    }

    public void ResetLevel()
    {
        UnPause();
        LevelManager.Instance.ResetLevel();
    }

    public void ReturnToMenu()
    {
        UnPause();
        LevelManager.Instance.ReturnToMenu();
    }

    private void OnCancel(InputValue value)
    {
        if(value.isPressed)
            TogglePause();
    }
}
