using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperState
{
    public List<SquareStack> squareStacks;
    public List<JointStack> jointStacks;

    //
    //Stack data
        //location
        //orientation
        //Squares
    //Joint data
        //location
        //joint
    //Player Data
    //Object data
        //parent square/side
    //Corner Data
        //location
        //joints
    //Fold data
        //numfolds
    //Special data
        //glowstick states


    public void SendToTarget()
    {
        
    }
}

// public abstract class PaperStateData
// {

// }

// public class

public class ObjectData
{
    public Vector3Int currentPosition;
    public Vector3Int targetPosition;
    public Vector3 displayPosition;

    public Quaternion currentRotation;
    public Quaternion targetRotation;
    public Quaternion displayRotation;
}
