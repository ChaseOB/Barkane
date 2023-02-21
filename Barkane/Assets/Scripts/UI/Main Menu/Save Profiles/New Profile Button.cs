using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewProfileButton : MonoBehaviour
{
    public string profileName;

    public void CreateProfileAndStartGame() {
        SaveSystem.CreateNewProfile(profileName);
        SaveSystem.SetMostRecentProfile();
        MainMenuManager.StartGame();
    }
}
