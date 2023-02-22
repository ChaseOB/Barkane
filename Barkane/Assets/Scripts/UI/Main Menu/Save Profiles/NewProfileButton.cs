using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewProfileButton : ProfileButton
{
    public TMP_InputField profileNameTextField;

    public GameObject button;
    public GameObject creationScreen;

    private string profileName;

    public void toggleCreationScreen(bool val)
    {
        creationScreen.SetActive(val);
        button.SetActive(!val);
    }

    public void CreateProfileAndStartGame() {
        if (profileName.Length == 0)
            return;
        SaveSystem.CreateNewProfile(profileName);
        SaveSystem.SetMostRecentProfile();
        MainMenuManager.StartGame();
    }

    public void OnTextFieldChangeText(string text)
    {
        if (text.Contains("\n"))
        {
            profileNameTextField.text = text.Replace("\n", "");
            profileName = profileNameTextField.text;

            if (profileName.Length == 0)
                return;
            CreateProfileAndStartGame();
        }
    }
}
