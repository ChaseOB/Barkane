using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperSqaure : MonoBehaviour
{
    public Vector3Int currentLocation;
    public Vector3Int targetLocation;
    public bool playerOccupied = false; //true if the player is on this square

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.CompareTag("Player"))
            playerOccupied = true;
    }
    
    private void OnTriggerExit(Collider other) {
        if(other.gameObject.CompareTag("Player"))
            playerOccupied = false;
    }
}
