using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum ActionCallEnum
{
    PLAYER,
    UNDO,
    REDO
}

public class PaperStateManager: Singleton<PaperStateManager>
{
   //State
    private PaperState paperState;
    public  PaperState PaperState => paperState;

    public Stack<Action> actionStack = new Stack<Action>();
    public Stack<Action> actionRedoStack = new Stack<Action>();
    public FoldAnimator foldAnimator;

    //Apply the action to transition to the next state
    // public PaperState ProcessAction(PaperState startState, Action action)
    // {
    //     return startState;
    // }\
    
    private void Awake()
    {
        InitializeSingleton();
    }

    public void SetPaperState(PaperState ps)
    {
        paperState = ps;
    }

    public void AddAndExecuteFold(FoldData fd)
    {
        ActionLockManager.Instance.TryTakeLock(this);
        actionRedoStack.Clear();
        FoldAction foldAction = new(fd);
        actionStack.Push(foldAction);
        ProcessFoldAction(foldAction, ActionCallEnum.PLAYER);
    }

    public void ProcessFoldAction(FoldAction foldAction, ActionCallEnum source)
    {
        //Step 1: find out which squares/joints need to be moved
        // FoldablePaper fp = FindObjectOfType<FoldablePaper>();
        // FoldObjects foldObjects = fp.FindFoldObjects();

        //Step 2: set target postiion on squares/joints
        FoldData fd = foldAction.foldData;
        Quaternion rotation = Quaternion.Euler(fd.axisVector * 90);

        foreach(FoldableObject fo in fd.foldObjects)
        {
            PositionData current = fo.currentPosition;
            Vector3Int l =   Vector3Int.RoundToInt(rotation * (current.location - fd.axisPosition) + fd.axisPosition);
            Quaternion r = rotation * current.rotation;
            Vector3 a = r * Vector3.up;
            PositionData target = new(l, r, a);
            fo.targetPosition = target;
        }

        //Step 3: figure out which stacks need to be split and split them

        //Step 4: figue out which stacks need to be merged and merge them


        //Step 5: animate fold
         if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();
        ActionLockManager.Instance.TryRemoveLock(this);
        foldAnimator.Fold(foldAction.foldData, paperState, source);
        //Step 6: align to position
    }

    public void UndoAction()
    {   
        if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
            print("cannot undo: lock taken");
            return;
        }
        if(actionStack.Count == 0){
            print("nothing to undo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        Action action = actionStack.Pop();
        if(action == null){
            print("nothing to undo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        actionRedoStack.Push(action);
        //ActionLockManager.Instance.TryRemoveLock(this);
        if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();
        if(action is FoldAction)
        {
            ProcessFoldAction((FoldAction)action.GetInverse(), ActionCallEnum.UNDO);
        }
        //foldAnimator.Fold(fd.GetInverse(), null, fromStack: true);
        //Debug.Log($"undid {action} from stack");
    }

    public void RedoFold()
    {   
        if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
            print("cannot undo: lock taken");
            return;
        }
        if(actionRedoStack.Count == 0){
            print("nothing to redo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        Action action = actionRedoStack.Pop();
        if(action == null){
            print("nothing to undo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        actionStack.Push(action);
       // ActionLockManager.Instance.TryRemoveLock(this);
        if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();
        if(action is FoldAction)
        {
            ProcessFoldAction((FoldAction)action, ActionCallEnum.REDO);
        }
    }

}

//C: can be a fold or a player movement
public abstract class Action
{
    public abstract Action GetInverse();
}

public class FoldAction: Action
{
    public FoldData foldData;

    public FoldAction(FoldData foldData)
    {
        this.foldData = foldData;
    }

    public override Action GetInverse()
    {
        return new FoldAction(foldData.GetInverse());
    }
    // public Vector3 axis;
    // public JointData foldJoint;
    // public List<JointData> axisJoints;
}
