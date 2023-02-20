using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SaveProfileButton : ProfileButton
{
    [SerializeField] private int profileIndex = -1;

    public TextMeshProUGUI profileNameText;
    public TextMeshProUGUI currentLevelText;
    public TextMeshProUGUI timeText;
    private SaveProfile profile;

   /* private void OnEnable() 
    {
       // ReadProfileFromSave();
        UpdateButton();
    }*/

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
    
   /* public void ReadProfileFromSave()
    {
        SerializableSaveProfile ssp = SaveSystem.GetSerializableSaveProfile(profileIndex);
        if (ssp != null)
            profile = ssp.ToSaveProfile();
        else
            profile = null;
        SaveSystem.SetProfile(profileIndex, profile);
    }

    public void OnClick()
    {
        if (profile == null)
        {
            SaveSystem.SetProfile(profileIndex, new SaveProfile("Test Profile"));
        }
        else
        {
                LoadThisProfile();
            
        }
    }

    private void LoadThisProfile()
    {
        SaveSystem.LoadSaveProfile(profileIndex);
    }

    public void DeleteThisProfile()
    {
        if (profile != null)
        {
            // TODO: seek confirmation
            SaveSystem.DeleteSaveProfile(profileIndex);
            profile = null;
            SaveSystem.SetProfile(profileIndex, profile);
            UpdateButton();
        }
    }*/
}
