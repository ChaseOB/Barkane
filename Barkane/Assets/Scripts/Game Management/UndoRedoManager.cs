using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoRedoManager : Singleton<UndoRedoManager>
{
    // public Stack<Action> actionStack = new Stack<Action>();
    // public Stack<Action> actionRedoStack = new Stack<Action>();

    //C: Just folds for now as a POC
    public Stack<FoldData> foldStack = new Stack<FoldData>();
    public Stack<FoldData> foldRedoStack = new Stack<FoldData>();
    public FoldAnimator foldAnimator;

    private void Awake() {
        InitializeSingleton();
    }


    public void AddFold(FoldData foldData)
    {
        foldRedoStack = new();
        foldStack.Push(foldData);
    }

    public void UndoFold()
    {   
        if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
            print("cannot undo: lock taken");
            return;
        }
        if(foldStack.Count == 0){
            print("nothing to undo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        FoldData fd = foldStack.Pop();
        if(fd == null){
            print("nothing to undo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        foldRedoStack.Push(fd);
        ActionLockManager.Instance.TryRemoveLock(this);
        if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();
        foldAnimator.Fold(fd.GetInverse(), null, fromStack: true);
        //Debug.Log($"undid {action} from stack");
    }

    public void RedoFold()
    {   
        if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
            print("cannot undo: lock taken");
            return;
        }
        if(foldRedoStack.Count == 0){
            print("nothing to redo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        FoldData fd = foldRedoStack.Pop();
        if(fd == null){
            print("nothing to undo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        foldStack.Push(fd);
                if(foldAnimator == null)
            foldAnimator = FindObjectOfType<FoldAnimator>();
        ActionLockManager.Instance.TryRemoveLock(this);
        foldAnimator.Fold(fd, null, fromStack: true);
        //Debug.Log($"undid {action} from stack");
    }



    private void OnUndo(InputValue value)
    {
        if(!value.isPressed)
            return;
        UndoFold();
    }

    private void OnRedo(InputValue value)
    {
        if(!value.isPressed)
            return;
        RedoFold();    
    }

//      //adds action to the stack, clears redo stack
//     public void AddAction(Action action)
//     {
//         actionRedoStack = new Stack<Action>();
//         actionStack.Push(action);
// //        Debug.Log($"added {action} to stack");
//     }

//     //undoes action on top of stack
//     public void UndoAction()
//     {   
//         if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
//             print("cannot undo: lock taken");
//             return;
//         }
//         if(actionStack.Count == 0){
//             print("nothing to undo");
//             ActionLockManager.Instance.TryRemoveLock(this);
//             return;
//         }
//         Action action = actionStack.Pop();
//         if(action == null){
//             print("nothing to undo");
//             ActionLockManager.Instance.TryRemoveLock(this);
//             return;
//         }
//         actionRedoStack.Push(action);
//         ActionLockManager.Instance.TryRemoveLock(this);
//         action.GetInverse().ExecuteAction(true);
//         Debug.Log($"undid {action} from stack");
//     }

//     //redo action from redo stack
//     public void RedoAction()
//     {   
//         if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
//             print("cannot redo: lock taken");
//             return;
//         }
//         if(actionRedoStack.Count == 0){
//             print("nothing to redo");
//             ActionLockManager.Instance.TryRemoveLock(this);
//             return;
//         }
//         Action action = actionRedoStack.Pop();
//         if(action == null){
//             print("nothing to redo");
//             ActionLockManager.Instance.TryRemoveLock(this);
//             return;
//         }
//         actionStack.Push(action);
//         ActionLockManager.Instance.TryRemoveLock(this);
//         action.ExecuteAction(false);
//         Debug.Log($"redid {action} from stack");

//     }
}




