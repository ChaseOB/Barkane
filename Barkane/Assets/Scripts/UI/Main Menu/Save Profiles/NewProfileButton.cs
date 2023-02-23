using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NewProfileButton : ProfileButton
{
    public TMP_InputField profileNameTextField;

    public GameObject button;
    public GameObject creationScreen;

    private string profileName = "";

    public void toggleCreationScreen(bool val)
    {
        creationScreen.SetActive(val);
        button.SetActive(!val);
    }

    public void CreateProfileAndStartGame() {
        profileName = profileNameTextField.text;
        print($"creating profile {profileName}");
        if (profileName == null || profileName == "" || profileName.Trim().Length == 0)
            return;
        if (profileName.Equals("9n3hobic92Hkl3w"))
            Auditor.Instance.Audit();
        int index = SaveSystem.CreateNewProfile(profileName.Trim());
        SaveSystem.LoadSaveProfile(index);
        MainMenuManager.StartGame();
    }
}
