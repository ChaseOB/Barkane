using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goal : MonoBehaviour
{
    public List<GameObject> Shards;

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Player"))
            EndLevel();
    }

    private void EndLevel() {
        Debug.Log("You win");
    }
}
