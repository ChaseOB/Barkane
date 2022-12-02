using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour, IThemedItem
{
    public int numShards;
    private int numShardsCollected;
    private bool goalActive = false;
    [SerializeField] private GameObject inactiveGoal;
    [SerializeField] private GameObject activeGoal;
    [SerializeField] private GameObject goalPlane;

    [SerializeField] private List<Material> swirlMaterials;

    private void Start() {
        ActivateGoal(numShardsCollected >= numShards);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Player") && goalActive)
            StartCoroutine(WaitToEndLevel());
    }

    //C: Used so player finishes moving
    private IEnumerator WaitToEndLevel() {
        yield return new WaitUntil(() => !ActionLockManager.Instance.IsLocked);
        EndLevel();
    }
    
    public void EndLevel() {
        LevelManager.Instance.EndLevel();
        UIManager.Instance.EndLevel();
    }

    public void UpdateTheme(Theme t) {
        activeGoal.GetComponent<MeshRenderer>().material = t.crystalMat;
        goalPlane.GetComponent<MeshRenderer>().material = swirlMaterials[(int)t.themeEnum];
    }

    public void CollectShard()
    {
        numShardsCollected++;
        //update shard display
        UIManager.UpdateShardCount(numShardsCollected, numShards);
        if(numShardsCollected >= numShards)
            ActivateGoal();
    }

    private void ActivateGoal(bool val = true)
    {
        goalActive = val;
        inactiveGoal.SetActive(!val);
        activeGoal.SetActive(val);
        goalPlane.SetActive(val);
    }
}
