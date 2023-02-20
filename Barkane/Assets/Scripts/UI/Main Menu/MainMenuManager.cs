using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{
    public static int gameStartScene = 1;

    public GameObject playMenu;
    public GameObject mainMenu;
    public GameObject levelSelect;
    public GameObject credits;
    public GameObject cosmetics;
    public GameObject profileSelect;


    public static void StartGame() {
        SceneManager.LoadScene(gameStartScene);
    }

    public void ToggleProfileSelectMenu(bool toggle) {
        profileSelect.SetActive(toggle);
        mainMenu.SetActive(!toggle);
    }


    public void LoadLevel(int level)
    {
        LevelManager.Instance.LoadLevel(level);
    }

    public void TogglePlayMenu(bool toggle)
    {
        playMenu.SetActive(toggle);
        mainMenu.SetActive(!toggle);
    }

    public void ToggleLevelSelect(bool toggle)
    {
        levelSelect.SetActive(toggle);
        playMenu.SetActive(!toggle);
    }

    public void ToggleCredits(bool toggle)
    {
        credits.SetActive(toggle);
        mainMenu.SetActive(!toggle); 
    }

    public void ToggleCosmetics(bool toggle)
    {
        cosmetics.SetActive(toggle);
        mainMenu.SetActive(!toggle); 
    }


    public void QuitGame() {
        Application.Quit();
    }

}
