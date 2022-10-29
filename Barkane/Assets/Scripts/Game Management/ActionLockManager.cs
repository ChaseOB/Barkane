using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLockManager : Singleton<ActionLockManager>
{
    private bool isLocked = false;
    public bool IsLocked => isLocked;
    private Object lockObject;
    public Object LockObject => lockObject;

    private void Awake() {
        InitializeSingleton();
    }


    //If the lock is open, takes the lock with the given object
    //returns true if the lock is open or the passed object is the same as the lock object
    public bool TryTakeLock(Object o)
    {
        if(isLocked && lockObject != o)
            return false;
        isLocked = true;
        lockObject = o;
        return true;
    }

    //C: You probably shouldn't ever call this
    public void ForceTakeLock(Object o)
    {
        isLocked = true;
        lockObject = o;
    }


    //C: removes lock if the given object is the lock object
    // returns true if lock removed or there is no lock, false o is not the lock object
    public bool TryRemoveLock(Object o)
    {
        if(o != lockObject)
            return false;
        lockObject = null;
        isLocked = false;
        return true;
    }

    public void ForceRemoveLock()
    {
        isLocked = false;
        lockObject = null;
    }
}
