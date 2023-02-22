using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuManager : Singleton<MainMenuManager>
{
    public static int gameStartScene = 1;

   // public GameObject playMenu;
    public GameObject mainMenu;
    public GameObject levelSelect;
    public GameObject credits;
    public GameObject cosmetics;
    public GameObject profileSelect;
    public GameObject currentProfile;

    public TextMeshProUGUI profileNameText;

    private void Awake() {
        InitializeSingleton();
    }

    public static void StartGame() {
       // SceneManager.LoadScene(gameStartScene);
        LevelManager.Instance.LoadLevel(0);
    }

    public void ToggleProfileSelectMenu(bool toggle) {
        profileSelect.SetActive(toggle);
        mainMenu.SetActive(!toggle);
    }


    public void LoadLevel(int level)
    {
        LevelManager.Instance.LoadLevel(level);
    }

    public void ToggleCredits(bool toggle)
    {
        credits.SetActive(toggle);
        mainMenu.SetActive(!toggle); 
    }

    public void ShowProfileScreen(string profileName)
    {
        currentProfile.SetActive(true);
        profileSelect.SetActive(false);
        profileNameText.text = profileName;
    }

    public void ReturnToProfileSelect()
    {
        currentProfile.SetActive(false);
        profileSelect.SetActive(true);
    }


    public void QuitGame() {
        Application.Quit();
    }

}
