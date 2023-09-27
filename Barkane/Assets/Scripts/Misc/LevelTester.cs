using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BarkaneEditor;


public class LevelTester : MonoBehaviour
{
    public Level level;
    public GameObject playerPrefab;

    private void Start() {
        Invoke("SpawnLevel", 0.02f);
    }

    public void SpawnLevel()
    {
        GameObject instantiatedLevel = Instantiate(level.levelObject, Vector3.zero, Quaternion.identity);
        FoldablePaper paper = instantiatedLevel.GetComponent<FoldablePaper>();
        Transform playerPos = paper.playerSpawn;
        GameObject playerInstance= Instantiate(playerPrefab, playerPos.position, Quaternion.identity);
        FollowTarget.Instance.SetTargetAndPosition(playerInstance.GetComponent<PlayerMovement>().cameraTrackingTransform);    
        VFXManager.Instance.Refresh();
        FindObjectOfType<TileSelector>().ReloadReferences();
    }
}