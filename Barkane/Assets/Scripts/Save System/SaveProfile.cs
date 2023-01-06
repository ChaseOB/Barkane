using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveProfile
{
    //C: Mostly stolen from Slider lol 
    // profile-specific metadata
    private string profileName;
    private bool completionStatus;
    private float playTimeInSeconds;
    private System.DateTime lastSaved;

    private int lastLevel;

    //Level name -> number of folds. -1 if not completed
    private Dictionary<string, int> numFolds = new Dictionary<string, int>();
    private Dictionary<string, bool> cosmeticUnlocks = new Dictionary<string, bool>();


    public SaveProfile(string profileName)
    {
        this.profileName = profileName;
    }

    #region Getters / Setters
    public string GetProfileName()
    {
        return profileName;
    }

    public bool GetCompletionStatus()
    {
        return completionStatus;
    }

    public void SetCompletionStatus(bool value)
    {
        completionStatus = value;
    }

    public float GetPlayTimeInSeconds()
    {
        return playTimeInSeconds;
    }

    public void SetPlayTimeInSeconds(float value)
    {
        playTimeInSeconds = value;
    }

    public void AddPlayTimeInSeconds(float time)
    {
        playTimeInSeconds += time;
    }

    public System.DateTime GetLastSaved()
    {
        return lastSaved;
    }

    public void SetLastSaved(System.DateTime value)
    {
        lastSaved = value;
    }

    public Dictionary<string, bool> GetCosmeticsDictionary()
    {
        return cosmeticUnlocks;
    }

    public void SetCosmeticsDictionary(Dictionary<string, bool> value)
    {
        cosmeticUnlocks = value;
    }

    public Dictionary<string, int> GetFoldsDictionary()
    {
        return numFolds;
    }

    public void SetFoldsDictionary(Dictionary<string, int> value)
    {
        numFolds = value;
    }
    #endregion

    #region Dictionaries
    public bool GetCosmeticUnlock(string name, bool defaultVal = false)
    {
        return cosmeticUnlocks.GetValueOrDefault(name, defaultVal);
    }

    public void SetCosmeticUnlock(string name, bool value)
    {
        cosmeticUnlocks[name] = value;
    }

    public int GetFolds(string levelName)
    {
        return numFolds.GetValueOrDefault(levelName, -1);
    }

    public void SetNumFolds(string levelName, int value)
    {
        numFolds[levelName] = value;
    }
    #endregion

}