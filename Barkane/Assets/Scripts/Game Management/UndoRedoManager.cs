using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UndoRedoManager : Singleton<UndoRedoManager>
{
    public Stack<Action> actionStack = new Stack<Action>();
    public Stack<Action> actionRedoStack = new Stack<Action>();

    private void Awake() {
        InitializeSingleton();
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

     //adds action to the stack, clears redo stack
    public void AddAction(Action action)
    {
        actionRedoStack = new Stack<Action>();
        actionStack.Push(action);
        Debug.Log($"added {action} to stack");
    }

    //undoes action on top of stack
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
        ActionLockManager.Instance.TryRemoveLock(this);
        action.GetInverse().ExecuteAction(true);
        Debug.Log($"undid {action} from stack");
    }

    //redo action from redo stack
    public void RedoAction()
    {   
        if(ActionLockManager.Instance.IsLocked || !ActionLockManager.Instance.TryTakeLock(this)){
            print("cannot redo: lock taken");
            return;
        }
        if(actionRedoStack.Count == 0){
            print("nothing to redo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        Action action = actionRedoStack.Pop();
        if(action == null){
            print("nothing to redo");
            ActionLockManager.Instance.TryRemoveLock(this);
            return;
        }
        actionStack.Push(action);
        ActionLockManager.Instance.TryRemoveLock(this);
        action.ExecuteAction(false);
        Debug.Log($"redid {action} from stack");

    }
}



//C: can be a fold or a player movement
//TODO: Allow for other things to happen during these actions (snowball move, glowstick counts, crystal collection, etc)
public abstract class Action
{
    public abstract Action GetInverse();
    public abstract void ExecuteAction(bool undo);
}
