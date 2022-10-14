using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : Singleton<UIManager>
{
    public TMP_Text shardCountText;
    public TMP_Text foldCountText;

    public GameObject shardCountGroup;
    public int menuIndex;
    public GameObject endLevelGroup;

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

    public void ResetCounts(int numShards = 0)
    {
        UpdateFC(0);
        UpdateSC(0, numShards);
    }

    public static void UpdateShardCount(int currShards, int totalShards)
    {
        Instance.UpdateSC(currShards, totalShards);
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
        Instance.UpdateFC(numFolds);
    }

    public void UpdateFC(int numFolds)
    {
        foldCountText.text = numFolds.ToString();
    }

    public void EndLevel()
    {
        endLevelGroup.SetActive(true);
    }

    public void LoadNextLevel()
    {
        endLevelGroup.SetActive(false);
        LevelManager.Instance.LoadNextLevel();
    }

    public void ReturnToMenu()
    {
        endLevelGroup.SetActive(false);
        LevelManager.Instance.UnloadLevel();
        SceneManager.LoadScene(menuIndex);
    }
}
