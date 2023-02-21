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

    public void SetProfile(SaveProfile profile)
    {
        this.profile = profile;
        UpdateButton();
    }
    private void UpdateButton() 
    {
        profileNameText.text = profile.GetProfileName();
        currentLevelText.text = profile.GetLastLevel();
        float seconds = profile.GetPlayTimeInSeconds();
        int minutes = (int)seconds / 60;
        timeText.text = string.Format("{0}h{1:D2}", minutes / 60, minutes % 60);
    }

    private void UpdateIcon(int worldnum) {
        worldIcon.sprite = worldIconSprites[worldnum];
    }

}
