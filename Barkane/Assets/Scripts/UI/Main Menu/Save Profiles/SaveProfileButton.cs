using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveProfileButton : ProfileButton
{
    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI levelNumText;
    public TextMeshProUGUI completionText;


    public List<Sprite> worldIconSprites;
    public Image worldIcon;

    private SaveProfile profile;
    private int index;

    private int numLevels = 9;

    public void SetProfile(SaveProfile profile, int ind)
    {
        this.profile = profile;
        index = ind;
        UpdateButton();
    }
    private void UpdateButton() 
    {
        profileNameText.text = profile.GetProfileName();
        currentLevelText.text = profile.GetLastLevelString();
        float seconds = profile.GetPlayTimeInSeconds();
        timeText.text = GetTimeText(seconds);
        levelNumText.text = profile.GetLastLevelNum().ToString();
        worldIcon.sprite = worldIconSprites[profile.GetLastLevelWorldNum()];
        int completionPercent = profile.GetNumLevelsCompleted() * 100 / numLevels;
        completionText.text = $"{completionPercent}%";
    }

    private string GetTimeText(float seconds)
    {
        string ret;
        int minutes = (int)seconds / 60;
        if(minutes > 60)
            ret = string.Format("{0} Hours\n{1:D2} Minutes", minutes / 60, minutes % 60);
        else
            ret = string.Format("{0:D2} Minutes", minutes % 60);
        return ret;
    }



    public void LoadProfile() {
       SaveSystem.SetCurrentProfile(index);
       MainMenuManager.Instance.ShowProfileScreen();
       CosmeticManager.Instance.SetCosmetics();
    }

    public void DeleteProfile(){
        SaveSystem.DeleteSaveProfile(index);
        profile = null;
        SaveSystem.SetProfile(index, profile);
        SaveManager.Instance.UpdateAllButtons();
    }

}
