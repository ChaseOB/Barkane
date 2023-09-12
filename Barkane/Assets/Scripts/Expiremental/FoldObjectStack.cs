using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldObjectStack : MonoBehaviour
{
    public Vector3Int coordinates;
    public Orientation orientation;

    //The "top" of the stack is in the positive direction on the axis and the 
    // "bottom" is in the negative direction
    public LinkedList<PaperSquare> squares;
}
