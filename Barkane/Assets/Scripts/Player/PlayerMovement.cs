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
    private Vector2 move;
 
    public AnimationCurve moveVertCurve;
    public float bounceHeight = 0.2f; 
    private float marmaladeY;

    public LayerMask playerCollidingMask;
    public LayerMask targetMask;
    public LayerMask snowballMask;
    public BoxCollider target;

    private Snowball snowball;
    private Animator animator;

    private bool invalidMoveAnim = false;


    private void Start() 
    {
        marmaladeY = marmalade.transform.position.y;
        animator = GetComponent<Animator>();
    }

    private void Update() {
       cameraTrackingTransform.position = new Vector3(marmalade.transform.position.x, marmaladeY, marmalade.transform.position.z);
    }

    #region input

    public void OnMove(InputValue value)
    {
        if(PauseManager.IsPaused) return;
        if(isMoving) return;
        if(!ActionLockManager.Instance.TryTakeLock(this)) return;
        
        Vector2 move = value.Get<Vector2>();
        if(move.y > 0.5)
            Move();
        else if (Mathf.Abs(move.x) > 0.5)
            Rotate(move.x > 0 ? 90 : -90);
        else
            ActionLockManager.Instance.TryRemoveLock(this);
    }

    #endregion


    #region movement

    public void Move(bool fromStack = false, bool undo = false)
    {
        if(undo || (CheckValidMove() && CheckValidSnowball())) {
            StartCoroutine(MoveHelper(fromStack, undo));
            return;
        }
        PlayInvalidMoveAnimation();
    }

    private bool CheckValidMove()
    {
        Collider[] colliders = Physics.OverlapBox(target.transform.position + target.center, target.size/2, Quaternion.identity, targetMask, QueryTriggerInteraction.Collide);
        bool validLoc = colliders.Length > 0;
        bool hit = Physics.Raycast(raycastStart.position, 
                                (target.transform.position + target.center - raycastStart.position), 
                                Vector3.Magnitude(target.transform.position + target.center - raycastStart.position), playerCollidingMask); 
        Debug.DrawRay(raycastStart.position, target.transform.position + target.center - raycastStart.position, Color.cyan, 15);
        return validLoc && !hit;
    }

    private bool CheckValidSnowball()
    {
        Collider[] colliders = Physics.OverlapBox(target.transform.position + target.center, target.size/2, Quaternion.identity, snowballMask, QueryTriggerInteraction.Collide);
        bool sbExists = colliders.Length > 0;
        if(!sbExists) {
            return true;
        }
        snowball = colliders[0].gameObject.GetComponent<Snowball>();
        return snowball.CheckIfCanPushSnowball(transform.forward);
    }

    private void PlayInvalidMoveAnimation() {
        if(invalidMoveAnim) return;
        invalidMoveAnim = true;
        animator.Play("MoveFail");
        AudioManager.Instance.Play("Fail");
    }

    private void OnEndMoveAnimation() {
        invalidMoveAnim = false;
        animator.Play("Idle");
        ActionLockManager.Instance.TryRemoveLock(this);
    }

    private IEnumerator MoveHelper(bool fromStack, bool undo)
    {
        isMoving = true;
        
        TileSelector.Instance.DeselectJoint();

        Vector3 curr = transform.position;
        Vector3 goal = targetPos.transform.position;
        if(undo){
            goal += 2 * (curr - goal);
        }

        if(snowball != null) {
            snowball.MoveSnowball();
            snowball = null;
        }
        animator.Play("Move");
        float t = 0;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(curr, goal, t/moveDuration);
            //marmalade.transform.position = new Vector3(marmalade.transform.position.x, 
           //                                 marmaladeY + moveVertCurve.Evaluate(t/moveDuration) * bounceHeight,  
           //                                 marmalade.transform.position.z);
            yield return null;
        }
        transform.position = goal;
        //marmalade.transform.position = new Vector3(marmalade.transform.position.x, 
        //                                    marmaladeY,  
           //                                 marmalade.transform.position.z);
        isMoving = false;
        if(!fromStack && !undo) {
            PlayerMove pm = new PlayerMove();
            pm.movetype = 1;
            UndoRedoManager.Instance?.AddAction(pm);
        }
        animator.Play("Idle");
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

