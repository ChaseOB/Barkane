using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaperStateManager : MonoBehaviour
{
   //State

    //Apply the action to transition to the next state
    public PaperState ProcessAction(PaperState startState, Action action)
    {
        return startState;
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
    public JointData foldJoint;
    public List<JointData> axisJoints;
}
