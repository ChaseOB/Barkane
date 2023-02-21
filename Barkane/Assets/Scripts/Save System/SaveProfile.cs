using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections; 
using System;

public class SaveProfile
{
    //C: Mostly stolen from Slider lol 

    private string profileName;
    private bool completionStatus;
    private float playTimeInSeconds;
    private System.DateTime lastSaved;
    private string lastLevel;
    private string cosmetic;

    //Level name -> number of folds. -1 if not completed
    private Dictionary<string, int> numFolds = new Dictionary<string, int>();
    //Level name -> unlocked?
    private Dictionary<string, bool> levelUnlocks = new Dictionary<string, bool>();
    //Cosmetic name -> unlocked?
    private Dictionary<string, bool> cosmeticUnlocks = new Dictionary<string, bool>();


    public SaveProfile(string profileName)
    {
        this.profileName = profileName;
        lastSaved = System.DateTime.Now;
        cosmeticUnlocks.Add("None", true);
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

    public string GetLastLevel()
    {
        return lastLevel;
    }

    public void SetLastLevel(string level)
    {
        lastLevel = level;
    }

    public string GetCosmetic()
    {
        return cosmetic;
    }

    public void SetCosmetic(string newCosmetic)
    {
        cosmetic = newCosmetic;
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

    public Dictionary<string, bool> GetLevelUnlocksDictionary()
    {
        return levelUnlocks;
    }

    public void SetLevelUnlocksDictionary(Dictionary<string, bool> value)
    {
        levelUnlocks = value;
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

    public bool SetNumFoldsIfLower(string levelName, int value)
    {
        if(!numFolds.ContainsKey(levelName))
        {
            numFolds[levelName] = value;
            return true;
        }
        if(value < numFolds[levelName])
        {
            numFolds[levelName] = value;
            return true;
        }
        return false;
    }

    public bool GetLevelUnlock(string levelName)
    {
        return levelUnlocks.GetValueOrDefault(levelName, false);
    }

    public void SetLevelUnlock(string levelName, bool value)
    {
        levelUnlocks[levelName] = value;
    }

    #endregion

    #region sorting

    public static IComparer SortMostRecent()
    {
        return (IComparer) new SortProfilesByMostRecentHelper();
    }

      
    

    private class SortProfilesByMostRecentHelper : IComparer
    {
        public int Compare(object a, object b)
        {
            SaveProfile s1 = (SaveProfile) a;
            SaveProfile s2 = (SaveProfile) b;
            if(s1 == null && s2 == null)
                return 0;
            if (s1 == null)
                return 1;
            if(s2 == null)
                return -1;
            return DateTime.Compare(s2.lastSaved, s1.lastSaved);
        }
    }
    #endregion

}