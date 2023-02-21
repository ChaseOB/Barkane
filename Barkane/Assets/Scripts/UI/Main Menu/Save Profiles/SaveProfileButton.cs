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

    public List<Sprite> worldIconSprites;
    public Image worldIcon;

    private SaveProfile profile;
    private int index;

    public void SetProfile(SaveProfile profile, int ind)
    {
        this.profile = profile;
        index = ind;
        UpdateButton();
    }
    private void UpdateButton() 
    {
        profileNameText.text = profile.GetProfileName();
        currentLevelText.text = profile.GetLastLevel();
        float seconds = profile.GetPlayTimeInSeconds();
        timeText.text = GetTimeText(seconds);
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

    private void UpdateIcon(int worldnum) {
        worldIcon.sprite = worldIconSprites[worldnum];
    }


    public void LoadProfile() {
       SaveSystem.SetCurrentProfile(index);
    }

}
