using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;
using System.Linq;


public enum ActionCallEnum
{
    NONE,
    PLAYER,
    UNDO,
    REDO
}

public class PaperStateManager: Singleton<PaperStateManager>
{
   //State
    public PaperState paperState;
    public  PaperState PaperState => paperState;

    public Stack<Action> actionStack = new Stack<Action>();
    public Stack<Action> actionRedoStack = new Stack<Action>();
    public FoldAnimator foldAnimator;
    public PlayerMovement playerMovement;
    
    public Dictionary<PaperSquare, SquareData> squareDict = new();
    public Dictionary<PaperJoint, JointData> jointDict = new();

    private int numFolds;
    public int NumFolds => numFolds;

    public class FoldArgs : System.EventArgs
    {
        public FoldData fd;
        public ActionCallEnum source;
        public int foldnum;

        public FoldArgs(FoldData fd, ActionCallEnum source, int foldnum)
        {
            this.fd = fd;
            this.source = source;
            this.foldnum = foldnum;
        }
    }

    public static event System.EventHandler<FoldArgs> OnFold;

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

     private void OnUndo(InputValue value)
    {
        if(!value.isPressed)
            return;
        UndoAction();
    }

    private void OnRedo(InputValue value)
    {
        if(!value.isPressed)
            return;
        RedoAction();    
    }

    public void AddAndExecuteMove(PlayerMove playerMove)
    {
        ActionLockManager.Instance.TryTakeLock(this);
        actionRedoStack.Clear();
        actionStack.Push(playerMove);
        ProcessMoveAction(playerMove, ActionCallEnum.PLAYER);
    }

    public void ProcessMoveAction(PlayerMove playerMove, ActionCallEnum source)
    {
        ActionLockManager.Instance.TryRemoveLock(this);
        playerMovement.Move(playerMove, source);
    }

    public void AddAndExecuteRotate(float amount)
    {
        ActionLockManager.Instance.TryTakeLock(this);
        PlayerRotate playerRotate = new(amount);
        actionRedoStack.Clear();
        actionStack.Push(playerRotate);
        ProcessRotateAction(playerRotate, ActionCallEnum.PLAYER);
    }

    public void ProcessRotateAction(PlayerRotate playerRotate, ActionCallEnum source)
    {
        ActionLockManager.Instance.TryRemoveLock(this);
        playerMovement.Rotate(playerRotate.amount, source);
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
        List<SquareStack> oldStacks = paperState.squareStacks;
        List<SquareStack> newStacks = new();

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
           // fo.targetPosition = target;
            if(fo is JointData)
            {
                JointData jd = (JointData)fo;
                jd = jointDict[jd.paperJoint];
                jd.SetTarget(target);
            }
            if(fo is SquareData)
            {
                SquareData sd = (SquareData)fo;
                sd = squareDict[sd.paperSquare];
                sd.SetTarget(target);
//                print("updating target for square at " + sd.currentPosition.location + " target " + target.location);
            }
//            print(fo + "target " + target);
        }

        //Step 3: figure out which stacks need to be split and split them
        List<SquareStack> remove = new();
        for(int i = 0; i < oldStacks.Count; i++)
        {
            SquareStack s = oldStacks[i];
            SquareStack newStack = s.SplitStack();
            if(newStack != null)
            {
                newStacks.Add(newStack);
            }
             if(s.IsEmpty)
                 remove.Add(s);
        }
        //THE SOURCE OF THE PROBLEM:
        // Newly generated stacks (from spliting) cannot be merged into properly
        // Seem to have bad targets

        foreach(SquareStack s in remove)
            oldStacks.Remove(s);
        
       // newStacks = newStacks.Where(s => !s.IsEmpty);
        
        // List<SquareStack> combined = new();
        // combined.AddRange(oldStacks);
        // combined.AddRange(news)

        //Step 4: figue out which stacks need to be merged and merge them
        //print("old stacks " + oldStacks.Count);
        //print("new stacks " + newStacks.Count);
        for(int i = 0; i < oldStacks.Count; i++)
        {
            SquareStack s1 = oldStacks[i];
            for(int j = 0; j < newStacks.Count; j++)
            {
                SquareStack s2 = newStacks[j];

                StackOverlapType overlap = s1.GetOverlap(s2);                
                //print(overlap);
                switch(overlap)
                {
                    case StackOverlapType.SAME:
                    case StackOverlapType.NONE:
                        break;
                    case StackOverlapType.BOTH:
                        break;
                    case StackOverlapType.START:
                        break;
                    case StackOverlapType.END:
                        s1.MergeIntoStack(s2, fd.axisVector);
                        break;
                }
            }
        }
        
       // newStacks = (List<SquareStack>)newStacks.Where(s => !s.IsEmpty);

        List<SquareStack> returnStacks = new();

        foreach(SquareStack s in newStacks)
        {
            if(!s.IsEmpty)
            {
                returnStacks.Add(s);
               // if(s.debug)
                   // print("new stack at " + s.currentPosition.location + " moved to " + s.targetPosition.location);
            }
            else
                if(s.debug)
                    print("removing new empty stack at " + s.currentPosition.location);
        }

        foreach(SquareStack s in oldStacks)
        {
             if(!s.IsEmpty)
            {
                returnStacks.Add(s);
               // if(s.debug)
                   // print("old stack at " + s.currentPosition.location + " moved to " + s.targetPosition.location);
            }
            else
                if(s.debug)
                    print("removing old empty stack at " + s.currentPosition.location);
        }

        foreach(SquareStack s in returnStacks)
        {
            s.UpdateYOffsets();
            s.DisableStackInsides();
        }

        paperState.squareStacks = returnStacks;
       // print("return stacks " + returnStacks.Count);


        //Step 5: animate fold
         if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();

        ActionLockManager.Instance.TryRemoveLock(this);

        if(source == ActionCallEnum.UNDO)
        {
            numFolds--;
        }
        else
        {
            numFolds++;
        }
        UIManager.Instance.UpdateFC(numFolds);
        LevelManager.Instance?.SetFoldCount(numFolds);
        

        // paperState.SendToTarget();
        // TileSelector.Instance.state = SelectState.NONE;
        OnFold?.Invoke(this, new(fd, source, numFolds));
        foldAnimator.Fold(fd, paperState, source);

        //TODO: make sure to disable stuff on the inside stacks (most importantly player location setter)
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
        if(action is PlayerMove)
        {
            ProcessMoveAction((PlayerMove)action.GetInverse(), ActionCallEnum.UNDO);
        }
        if(action is PlayerRotate)
        {
            ProcessRotateAction((PlayerRotate)action.GetInverse(), ActionCallEnum.UNDO);
        }
        //foldAnimator.Fold(fd.GetInverse(), null, fromStack: true);
        //Debug.Log($"undid {action} from stack");
    }

    public void RedoAction()
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
        if(action is PlayerMove)
        {
            ProcessMoveAction((PlayerMove)action, ActionCallEnum.REDO);
        }
        if(action is PlayerRotate)
        {
            ProcessRotateAction((PlayerRotate)action, ActionCallEnum.REDO);
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
}
