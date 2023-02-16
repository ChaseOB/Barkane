using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class UIManager : Singleton<UIManager>
{
    public TMP_Text shardCountText;
    public TMP_Text foldCountText;
    public TMP_Text yourFoldCountText;
    public TMP_Text bestFoldCountText;

    public GameObject shardCountGroup;
    public int menuIndex;
    public GameObject endLevelGroup;
    public GameObject inGameGroup;

    private void Awake() {
        InitializeSingleton();
    }

    private void Start() {
        Goal g = FindObjectOfType<Goal>();
        if(g != null)
            ResetCounts(g.numShards);
        else
            ResetCounts();
    }

    public void ToggleLevelGroup(bool val)
    {
        inGameGroup.SetActive(val);
    }

    public void ResetCounts(int numShards = 0)
    {
        UpdateFC(0);
        UpdateSC(0, numShards);
    }

    public static void UpdateShardCount(int currShards, int totalShards)
    {
        Instance?.UpdateSC(currShards, totalShards);
    }

    public void UpdateSC(int currShards, int totalShards)
    {
        if(totalShards == 0) {
            shardCountGroup.SetActive(false);
            return;
        }

        shardCountGroup.SetActive(true);
        shardCountText.text = $"{currShards}/{totalShards}";
    }

    public static void UpdateFoldCount(int numFolds)
    {
        if(Instance != null)
            Instance.UpdateFC(numFolds);
    }

    public void UpdateFC(int numFolds)
    {
        foldCountText.text = numFolds.ToString();
        yourFoldCountText.text = numFolds.ToString();
    }

    public void EndLevel()
    {
        //C: This is a horrible way to do this. I don't care
        bestFoldCountText.text = SaveSystem.Current.GetFolds(LevelManager.Instance.GetCurrentLevel().levelName).ToString();
        Time.timeScale = 0;
        endLevelGroup.SetActive(true);
    }

    public void ToggleEndLevelGroup(bool val)
    {
        endLevelGroup.SetActive(val);
    }

    public void ResetLevel()
    {
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
        Time.timeScale = 1;
        LevelManager.Instance.ResetLevel();
        endLevelGroup.SetActive(false);
    }

    public void LoadNextLevel()
    {
        EventSystem.current.GetComponent<EventSystem>().SetSelectedGameObject(null);
        Time.timeScale = 1;
        LevelManager.Instance.LoadNextLevel();
        endLevelGroup.SetActive(false);
    }

    public void ReturnToMenu()
    {
        endLevelGroup.SetActive(false);
        LevelManager.Instance.UnloadLevel();
        SceneManager.LoadScene(menuIndex);
    }
}
