using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveDuration = 0.25f;
    public float turnDuration = 0.25f;

    [SerializeField] private GameObject targetPos;
    public Transform raycastStart;
    [SerializeField] private GameObject marmalade;
    public Transform cameraTrackingTransform;

    private bool isMoving = false;
    private bool validMoveLoc = false; //true if the tile in front of the player is a valid move location
    private Vector2 move;
 
    public AnimationCurve moveVertCurve;
    public float bounceHeight = 0.2f; 
    private float marmaladeY;

    public LayerMask playerCollidingMask;

    private void Start() 
    {
        marmaladeY = marmalade.transform.position.y;
        cameraTrackingTransform = raycastStart;    
    }

    #region input

    public void OnMove(InputValue value)
    {
        Vector2 move = value.Get<Vector2>();
        if(isMoving) return;
        if(!ActionLockManager.Instance.TryTakeLock(this)) return;
        if(move.y > 0.5)
            Move();
        else if (Mathf.Abs(move.x) > 0.5)
            Rotate(move.x > 0 ? 90.0f : -90.0f);
        else
            ActionLockManager.Instance.TryRemoveLock(this);

    }

    #endregion

    #region movement

    public void SetValidMoveLoc(bool value)
    {
        validMoveLoc = value; 
    }

    public bool CheckValidMove()
    {
        return validMoveLoc && 
                !Physics.Raycast(raycastStart.position, 
                                (targetPos.transform.position + new Vector3(0, 0.05f ,0) - raycastStart.position), 
                                3.0f, playerCollidingMask); 
        //C:If the raycast is true, then there is something in between the player and the movable location
    }

    public void Move(bool fromStack = false, bool undo = false)
    {
        if(undo || CheckValidMove())
            StartCoroutine(MoveHelper(fromStack, undo));
    }

    private IEnumerator MoveHelper(bool fromStack, bool undo)
    {
        isMoving = true;
        Vector3 curr = transform.position;
        Vector3 goal = targetPos.transform.position;
        if(undo){
            goal += 2 * (curr - goal);
        }
        float t = 0;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(curr, goal, t/moveDuration);
            marmalade.transform.position = new Vector3(marmalade.transform.position.x, 
                                            marmaladeY + moveVertCurve.Evaluate(t/moveDuration) * bounceHeight,  
                                            marmalade.transform.position.z);
            yield return null;
        }
        transform.position = goal;
        isMoving = false;
        if(!fromStack && !undo) {
            PlayerMove pm = new PlayerMove();
            pm.movetype = 1;
            UndoRedoManager.Instance?.AddAction(pm);
        }
        ActionLockManager.Instance.TryRemoveLock(this);
    }

    public void Rotate(float degrees, bool fromStack = false, bool undo = false)
    {
        if(ActionLockManager.Instance.TryTakeLock(this))
            StartCoroutine(RotateHelper(degrees, fromStack, undo));
    }

    private IEnumerator RotateHelper(float degrees, bool fromStack, bool undo)
    {
        Quaternion currRot = transform.rotation;
        isMoving = true;
        float t = 0;
        while (t < turnDuration)
        {
            t += Time.deltaTime;
            transform.RotateAround(transform.position, Vector3.up, (degrees / turnDuration) * Time.deltaTime);
            yield return null;
        }
        transform.rotation = currRot * Quaternion.Euler(0, degrees, 0);
        isMoving = false;
        if(!fromStack && !undo) {
            PlayerMove pm = new PlayerMove();
            pm.movetype = (degrees > 0 ? 0: 2);
            UndoRedoManager.Instance?.AddAction(pm);
        }
        ActionLockManager.Instance.TryRemoveLock(this);
    }

    #endregion
    
}

public class PlayerMove: Action 
{
    //C: 0 = rotate right, 1 = move forward, 2 = rotate left, 3 = move back
    public int movetype = 0;
    public override Action GetInverse()
    {
        PlayerMove pm = (PlayerMove)this.MemberwiseClone();
        pm.movetype = (this.movetype + 2) % 4;

        return (Action) pm;
    }

    public override void ExecuteAction(bool undo)
    {
        PlayerMovement pm = GameObject.FindObjectOfType<PlayerMovement>();
        if(movetype == 0)
            pm.Rotate(90, true, undo);
        else if(movetype == 2)
            pm.Rotate(-90, true, undo);
        else{
            pm.Move(true, undo);
        }
    }
}

