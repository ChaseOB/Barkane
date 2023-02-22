using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

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
    public GameObject deleteMenu;



    public List<Sprite> worldIconSprites;
    public Image worldIcon;
    public TextMeshProUGUI profileNameText;

    private SaveProfile profile;
    private int profileIndex;

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

    public void ShowProfileScreen(int index)
    {
        profile = SaveSystem.Current;
        profileIndex = index;
        profileNameText.text = profile.GetProfileName();
        worldIcon.sprite = worldIconSprites[profile.GetLastLevelWorldNum()];
        currentProfile.SetActive(true);
        profileSelect.SetActive(false);
    }

    public void ToggleDeleteScreen(bool toggle)
    {
        currentProfile.SetActive(!toggle);
        deleteMenu.SetActive(toggle);
    }

    public void DeleteProfile(){
        SaveSystem.DeleteSaveProfile(profileIndex);
        profile = null;
        SaveSystem.SetProfile(profileIndex, profile);
        SaveManager.Instance.UpdateAllButtons();
        deleteMenu.SetActive(false);
        profileSelect.SetActive(true);
    }

    public void ReturnToProfileSelect()
    {
        currentProfile.SetActive(false);
        profileSelect.SetActive(true);
    }

    public void ToggleCosmeticsMenu(bool toggle)
    {
        cosmetics.SetActive(toggle);
        currentProfile.SetActive(!toggle);
    }

    public void ToggleLevelSelect(bool toggle)
    {
        levelSelect.SetActive(toggle);
        currentProfile.SetActive(!toggle);
    }

    public void ContinueGame() {
        LevelManager.Instance.LoadLevel(profile.GetLastLevelString());
    }


    public void QuitGame() {
        Application.Quit();
    }

}
