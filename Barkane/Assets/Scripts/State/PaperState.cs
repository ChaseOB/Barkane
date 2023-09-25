using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PaperState
{
    public List<SquareStack> squareStacks = new();
    public List<JointStack> jointStacks = new();
    // public Vector2 playerPosition = Vector2.zero;
    // public Quaternion playerRotation;
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
        foreach(SquareStack s in squareStacks)
        {
            s.SendToTarget();
        }
        foreach(JointStack j in jointStacks)
        {
            j.SendToTarget();
        }
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
   // public Vector3 displayPosition;

    public Quaternion currentRotation;
    public Quaternion targetRotation;
   // public Quaternion displayRotation;
}
