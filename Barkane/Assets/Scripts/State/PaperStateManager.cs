using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class PaperStateManager: MonoBehaviour
{
   //State
    private static PaperState paperState;
    public static PaperState PaperState => paperState;

    //Apply the action to transition to the next state
    // public PaperState ProcessAction(PaperState startState, Action action)
    // {
    //     return startState;
    // }

    public static void SetPaperState(PaperState ps)
    {
        paperState = ps;
    }

    public static PaperState ProcessFoldAction(PaperState paperState, FoldAction foldAction)
    {
        //Step 1: find out which squares/joints need to be moved
        FoldablePaper fp = FindObjectOfType<FoldablePaper>();
        FoldObjects foldObjects = fp.FindFoldObjects();

        //Step 2: set target postiion on squares/joints

        Quaternion rotation = Quaternion.Euler(foldAction.axis * 90);

        foreach(FoldableObject fo in foldObjects.foldSideObjects)
        {
            PositionData current = fo.currentPosition;
            Vector3 axisPosition = foldAction.foldJoint.currentPosition.location;
            Vector3Int l =   Vector3Int.RoundToInt(rotation * (current.location - axisPosition) + axisPosition);
            Quaternion r = rotation * current.rotation;
            Vector3 a = r * Vector3.up;
            PositionData target = new(l, r, a);
            fo.targetPosition = target;
        }

        //Step 3: figure out which stacks need to be split and split them

        //Step 4: figue out which stacks need to be merged and merge them


        return paperState;
    }

}

//C: can be a fold or a player movement
public abstract class Action
{
    //public abstract Action GetInverse();
    //public abstract void ExecuteAction(bool undo);
}

public class FoldAction: Action
{
    public Vector3 axis;
    public JointData foldJoint;
    public List<JointData> axisJoints;
}
