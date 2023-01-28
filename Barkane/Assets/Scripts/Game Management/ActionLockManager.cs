using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionLockManager : Singleton<ActionLockManager>
{
    private bool isLocked = false;
    public bool IsLocked => isLocked;
    public Object lockObject;
    public Object LockObject => lockObject;

    private void Awake() {
        InitializeSingleton(this);
    }


    // If the lock is open, takes the lock with the given object
    // Returns true if the lock is open or the passed object is the same as the lock object
    public bool TryTakeLock(Object o)
    {
        if(isLocked && lockObject != o)
            return false;
        isLocked = true;
        lockObject = o;
        return true;
    }

    // Used to forcefully take the lock for scene loading
    // You probably should not call this otherwise
    public void ForceTakeLock(Object o)
    {
        isLocked = true;
        lockObject = o;
    }


    // Removes lock if the given object is the lock object
    // Returns true if lock removed or there is no lock, false if o is not the lock object
    public bool TryRemoveLock(Object o)
    {
        if(o != lockObject)
            return false;
        lockObject = null;
        isLocked = false;
        return true;
    }

    //Removes the lock
    public void ForceRemoveLock()
    {
        isLocked = false;
        lockObject = null;
    }
}
