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

    public ActionCallEnum source = ActionCallEnum.NONE;

    private void Start() 
    {
        marmaladeY = marmalade.transform.position.y;
        animator = GetComponent<Animator>();
        PaperStateManager.Instance.playerMovement = this;
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
        {
            if(CheckValidMove() && CheckValidSnowball())
            {
                ActionLockManager.Instance.TryRemoveLock(this);
                PaperStateManager.Instance.AddAndExecuteMove(true);
            }
            else
            {
                PlayInvalidMoveAnimation();
            }
        }
            //Move();
        else if (Mathf.Abs(move.x) > 0.5)
        {
            ActionLockManager.Instance.TryRemoveLock(this);
            PaperStateManager.Instance.AddAndExecuteRotate(move.x > 0 ? 90 : -90);
        }
            //Rotate(move.x > 0 ? 90 : -90);
        else
            ActionLockManager.Instance.TryRemoveLock(this);
    }

    #endregion



    #region movement


    public void Move(PlayerMove move, ActionCallEnum source)
    {
        ActionLockManager.Instance.TryTakeLock(this);
        StartCoroutine(MoveHelper(move, source));
        // if(undo || (CheckValidMove() && CheckValidSnowball())) {
        //     StartCoroutine(MoveHelper(source));
        //     return;
        // }
        // PlayInvalidMoveAnimation();
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

    private IEnumerator MoveHelper(PlayerMove move, ActionCallEnum source)
    {
        isMoving = true;
        this.source = source;
        
        TileSelector.Instance.DeselectJoint();

        Vector3 curr = transform.position;
        Vector3 goal = targetPos.transform.position;
        if(source == ActionCallEnum.UNDO){
            goal += 2 * (curr - goal);
        }

        CheckValidSnowball();
        if(snowball != null) {
            snowball.MoveSnowball(move, source);
            snowball = null;
        }
        animator.Play("Move");
        float t = 0;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(curr, goal, t/moveDuration);

            yield return null;
        }
        transform.position = goal;
        isMoving = false;
        // if(!fromStack && !undo) {
        //     PlayerMove pm = new PlayerMove();
        //     pm.movetype = 1;
        //     //UndoRedoManager.Instance?.AddAction(pm);
        // }
        this.source = ActionCallEnum.NONE;
        animator.Play("Idle");
        ActionLockManager.Instance.TryRemoveLock(this);
    }

    public void Rotate(float degrees, ActionCallEnum source)
    {
        if(ActionLockManager.Instance.TryTakeLock(this))
            StartCoroutine(RotateHelper(degrees, source));
    }

    private IEnumerator RotateHelper(float degrees, ActionCallEnum source)
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
        // if(!fromStack && !undo) {
        //     PlayerMove pm = new PlayerMove();
        //     pm.movetype = (degrees > 0 ? 0: 2);
        //    // UndoRedoManager.Instance?.AddAction(pm);
        // }
        ActionLockManager.Instance.TryRemoveLock(this);
    }

    #endregion
}

public class PlayerMove: Action 
{
    public bool forward = true;

    public PlayerMove(bool forward, int moveNum)
    {
        this.forward = forward;
        this.moveNum = moveNum;
    }

    public override Action GetInverse()
    {
        return new PlayerMove(!forward, moveNum);
    }

    // public void ExecuteAction(bool undo)
    // {
    //     PlayerMovement pm = GameObject.FindObjectOfType<PlayerMovement>();
    //     if(movetype == 0)
    //         pm.Rotate(90, true, undo);
    //     else if(movetype == 2)
    //         pm.Rotate(-90, true, undo);
    //     else{
    //         pm.Move(true, undo);
    //     }
    // }
}

public class PlayerRotate : Action
{
    public float amount;

    public PlayerRotate(float amount, int moveNum)
    {
        this.amount = amount;
        this.moveNum = moveNum;
    }

    public override Action GetInverse()
    {
        return new PlayerRotate(-1 * amount, moveNum);
    }
}

