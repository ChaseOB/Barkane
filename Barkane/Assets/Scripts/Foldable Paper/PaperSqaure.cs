using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperSqaure : MonoBehaviour
{
    [SerializeField] private bool isFoldable; //testing for now
    [SerializeField] private Vector3Int orientation; //the unit normal vector to this paper square
    [SerializeField] private bool[] foldableDirections = new bool[4]; //what directions (ENWS) can be folded, relative to this square?
    [SerializeField] private PaperSqaure[] neighbors = new PaperSqaure[4]; //the east, north, west, and south neighbor of this square
   // [SerializeField] private float[] neighborRotations = new float[4]; //the rotation of each neighbor relative to this square, in degrees. Ranges from -180 to +180
    [SerializeField] private LineRenderer[] edges = new LineRenderer[4]; //the east, north, west, and south line renderers of this square, used to show folds






    // Start is called before the first frame update
    void Start()
    {
        
    }

    //Gets the neighbors of this square
    private void GetNeighbors()
    {

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
