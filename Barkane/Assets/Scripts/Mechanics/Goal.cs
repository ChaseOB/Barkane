using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public int numShards;
    private int numShardsCollected;
    private bool goalActive = false;
    [SerializeField] private GameObject inactiveGoal;
    [SerializeField] private GameObject activeGoal;
    public GameObject showOnWin;

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Player") && goalActive)
            EndLevel();
    }

    private void EndLevel() {
        showOnWin.SetActive(true);
    }

    public void CollectShard()
    {
        numShardsCollected++;
        //update shard display
        UIManager.UpdateShardCount(numShardsCollected, numShards);
        if(numShardsCollected >= numShards)
            ActivateGoal();
    }

    private void ActivateGoal()
    {
        goalActive = true;
        inactiveGoal.SetActive(false);
        activeGoal.SetActive(true);
    }
}
