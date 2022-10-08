using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EdgeParticles))]
public class PaperSqaure : MonoBehaviour
{
    [SerializeField] private bool playerOccupied = false; //true if the player is on this square
    public bool PlayerOccupied { get => playerOccupied;}

    
    //Visuals
    [SerializeField] private GameObject topHalf;
    public GameObject TopHalf => topHalf;
    [SerializeField] private GameObject bottomHalf;
    public GameObject BottomHalf => bottomHalf;
    [SerializeField] private EdgeParticles edgeParticles;

    private void Start() 
    {
        edgeParticles = GetComponent<EdgeParticles>();
    }

    public void SetPlayerOccupied(bool value)
    {
        playerOccupied = value;
    }

    //select is true when this region is selected and false when deselected
    public void OnFoldHighlight(bool select)
    {
        if(select)
            edgeParticles?.Emit();
        else
            edgeParticles?.Unemit();
    }

    //foldStart is true when starting a fold and false when ending a fold
    public void OnFold(bool foldStart)
    {
    }

}
