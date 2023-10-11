using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


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

    public int currMoveNum => actionStack.Count;
    private int nextMoveNum => actionStack.Count + 1;

    private PlayerActionHints hints;
    public class FoldArgs : System.EventArgs
    {
        public FoldData fd;
        public ActionCallEnum source;
        public int beforeFoldNum;
        public int afterFoldNum;

        public FoldArgs(FoldData fd, ActionCallEnum source, int beforeFoldNum, int afterFoldNum)
        {
            this.fd = fd;
            this.source = source;
            this.beforeFoldNum = beforeFoldNum;
            this.afterFoldNum = afterFoldNum;
        }
    }

   // public static event System.EventHandler<FoldArgs> OnFold;
    public static event System.EventHandler<FoldArgs> OnFoldStart;
    public static event System.EventHandler<FoldArgs> OnFoldEnd;

    //Apply the action to transition to the next state
    // public PaperState ProcessAction(PaperState startState, Action action)
    // {
    //     return startState;
    // }\
    
    private void Awake()
    {
        InitializeSingleton();
    }

    private void Start()
    {
        hints = FindObjectOfType<PlayerActionHints>();
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
        if(hints != null)
            hints.DisableHint("undo");
    }

    private void OnRedo(InputValue value)
    {
        if(!value.isPressed)
            return;
        RedoAction();
        if(hints != null)
            hints.DisableHint("redo");    
    }

    public void AddAndExecuteMove(bool forward)
    {
        PlayerMove playerMove = new(forward, nextMoveNum);
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
        PlayerRotate playerRotate = new(amount, nextMoveNum);
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
        FoldAction foldAction = new(fd, nextMoveNum);
        actionStack.Push(foldAction);
        ProcessFoldAction(foldAction, ActionCallEnum.PLAYER);
        PlayerActionHints hints = FindObjectOfType<PlayerActionHints>();
        if(hints != null)
        {
            hints.DisableHint("fold");
        }
    }

    public void ProcessFoldAction(FoldAction foldAction, ActionCallEnum source)
    {
        List<SquareStack> oldStacks = paperState.squareStacks;
        List<SquareStack> newStacks = new();

        //Step 1: Set target positions on all fold objects
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
            }
        }

        //Step 2: figure out which stacks need to be split and split them
        List<SquareStack> remove = new();
        for(int i = 0; i < oldStacks.Count; i++)
        {
            SquareStack s = oldStacks[i];
            SquareStack newStack = s.SplitStack(fd.axisVector);
            if(newStack != null)
            {
                newStacks.Add(newStack);
            }
             if(s.IsEmpty)
                 remove.Add(s);
        }

        foreach(SquareStack s in remove)
            oldStacks.Remove(s);


        //Step 4: figue out which stacks need to be merged and merge them
        for(int i = 0; i < oldStacks.Count; i++)
        {
            SquareStack s1 = oldStacks[i];
            for(int j = 0; j < newStacks.Count; j++)
            {
                SquareStack s2 = newStacks[j];

                StackOverlapType overlap = s1.GetOverlap(s2);                
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
        

        List<SquareStack> returnStacks = new();

        foreach(SquareStack s in newStacks)
        {
            if(!s.IsEmpty)
            {
                returnStacks.Add(s);
            }
        }

        foreach(SquareStack s in oldStacks)
        {
            if(!s.IsEmpty)
            {
                returnStacks.Add(s);
            }
        }

        foreach(SquareStack s in returnStacks)
        {
            s.UpdateYOffsets();
            s.DisableStackInsides();
        }

        paperState.squareStacks = returnStacks;
       // print("return stacks " + returnStacks.Count);


        //Step 4: animate fold
         if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();

        ActionLockManager.Instance.TryRemoveLock(this);

        int prevFold = source == ActionCallEnum.UNDO ? numFolds - 1 : numFolds;
        int nextFold = prevFold + 1;
        FoldArgs args = new(fd, source, prevFold, nextFold);
        void start() => OnFoldStartInternal(source, args);
        void end() => OnFoldEndInternal(source, args);
        var first = source == ActionCallEnum.UNDO ? (System.Action)end : start;
        var last = source == ActionCallEnum.UNDO ? start : (System.Action)end;
        //var foldStartAction = source == ActionCallEnum.UNDO ? OnFoldStartInternal(source, args) : OnFoldEndInternal(source, args);
        //OnFold?.Invoke(this, new(fd, source, numFolds));
        foldAnimator.Fold(fd, paperState, source, first, last);

    }

    private void OnFoldStartInternal(ActionCallEnum source, FoldArgs e)
    {
        numFolds = source == ActionCallEnum.UNDO ? e.beforeFoldNum : e.afterFoldNum;
        UIManager.Instance?.UpdateFC(numFolds);
        LevelManager.Instance?.SetFoldCount(numFolds);

        OnFoldStart?.Invoke(this, e);
    }

    private void OnFoldEndInternal(ActionCallEnum source, FoldArgs e)
    {
        OnFoldEnd?.Invoke(this, e);
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
    public int moveNum = -1;
    public abstract Action GetInverse();
}

public class FoldAction: Action
{
    public FoldData foldData;

    public FoldAction(FoldData foldData, int moveNum)
    {
        this.foldData = foldData;
        this.moveNum = moveNum;
    }

    public override Action GetInverse()
    {
        return new FoldAction(foldData.GetInverse(), moveNum);
    }
}
