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
    private string lastLevel = "cutscene";
    private string cosmetic;
    private int lastLevelWorldNum;
    private int lastLevelNum;
    private System.DateTime lastImproved;

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
        levelUnlocks.Add("Cb 1", true);
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

     public System.DateTime GetLastImproved()
    {
        return lastImproved;
    }

    public void SetLastImproved(System.DateTime value)
    {
        lastImproved = value;
    }

    public string GetLastLevelString()
    {
        return lastLevel;
    }

    public void SetLastLevelString(String level)
    {
        lastLevel = level;
    }



    public void SetLastLevel(Level level)
    {
        if(level == null) return;
        lastLevel = level.levelName;
        lastLevelNum = level.levelNum;
        lastLevelWorldNum = level.worldNum;
    }

    public int GetLastLevelWorldNum()
    {
        return lastLevelWorldNum;
    }

    public void SetLastLevelNum(int num)
    {
        lastLevelNum = num;
    }

    public int GetLastLevelNum()
    {
        return lastLevelNum;
    }

    public void SetLastLevelWorldNum(int worldNum)
    {
        lastLevelWorldNum = worldNum;
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
            lastImproved = System.DateTime.Now;
            return true;
        }
        if(value < numFolds[levelName])
        {
            numFolds[levelName] = value;
            lastImproved = System.DateTime.Now;
            return true;
        }
        return false;
    }

    public int GetNumLevelsCompleted()
    {
        return numFolds.Keys.Count();
    }

    public bool GetLevelUnlock(string levelName)
    {
        return levelUnlocks.GetValueOrDefault(levelName, false);
    }

    public void UnlockLevel(string level)
    {
        if(!levelUnlocks.ContainsKey(level))
            levelUnlocks.Add(level, true);
        else
            levelUnlocks[level] = true;
        Debug.Log("Unlocked level " + level);
    }

    public void SetLevelUnlock(string levelName, bool value)
    {
        levelUnlocks[levelName] = value;
    }
    
    public int GetNumFolds()
    {
        int folds = 0;
        foreach(int num in numFolds.Values) {
            if(num >=0)
                folds += num;
        }
        return folds;
    }
    #endregion

    public string GetProfileInfo()
    {
        int folds = GetNumFolds();
        int numLevels = GetNumLevelsCompleted();
        string ret = $"Name: {profileName} \n" +
                    $"Levels Completed: {numLevels} \n" +
                    $"Total Folds: {folds} \n" +
                    $"Last Played: {lastImproved}";
        return ret;
    }

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

    public static IComparer SortMostLevelsFewestFolds()
    {
        return (IComparer) new SortProfilesByMostLevelsAndFewestFolds();
    }
    

    private class SortProfilesByMostLevelsAndFewestFolds : IComparer
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
            
            int s1Levels = s1.GetNumLevelsCompleted();
            int s2Levels = s2.GetNumLevelsCompleted();

            //farther is better
            if(s1Levels != s2Levels)
                return s2Levels - s1Levels;
            
            int s1Folds = s1.GetNumFolds();
            int s2Folds = s2.GetNumFolds();

            //fewer folds is better
            if(s1Folds != s2Folds)
                return s1Folds - s2Folds;

            //if all else fails, give it to the person who did it first
            return DateTime.Compare(s1.lastImproved, s2.lastImproved);
        }
    }

    #endregion

}