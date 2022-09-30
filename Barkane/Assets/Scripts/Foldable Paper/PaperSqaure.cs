using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperSqaure : MonoBehaviour
{
    [SerializeField] private bool playerOccupied = false; //true if the player is on this square
    public bool PlayerOccupied { get => playerOccupied;}

    
    //Visuals
    [SerializeField] private GameObject topHalf;
    public GameObject TopHalf => topHalf;
    [SerializeField] private GameObject bottomHalf;
    public GameObject BottomHalf => bottomHalf;

    public void SetPlayerOccupied(bool value)
    {
        playerOccupied = value;
    }
}
