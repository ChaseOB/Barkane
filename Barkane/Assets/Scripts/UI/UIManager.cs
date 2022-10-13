using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class UIManager : Singleton<UIManager>
{
    public TMP_Text shardCountText;
    public TMP_Text foldCountText;

    public GameObject shardCountGroup;

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
}
