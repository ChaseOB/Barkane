using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float moveDuration = 0.25f;
    public float turnDuration = 0.25f;

    [SerializeField] private GameObject targetPos;
    [SerializeField] private Transform raycastStart;

    private bool isMoving = false;
    private bool validMoveLoc = false;
    private Vector2 move;
 
    public AnimationCurve moveVertCurve;
    public float jumpHeight = 0.2f;

    public LayerMask playerCollidingMask;

    #region input

    public void OnMove(InputValue value)
    {
        Vector2 move = value.Get<Vector2>();
        if(isMoving) return;
        if(move.y > 0.5)
            Move();
        else if (Mathf.Abs(move.x) > 0.5)
            Rotate(move.x > 0 ? 90.0f : -90.0f);
    }

    #endregion

    #region movement

    public void SetValidMoveLoc(bool value)
    {
        validMoveLoc = value; 
    }

    public void CheckValidMove()
    {
        validMoveLoc = !Physics.Raycast(raycastStart.position, targetPos.transform.position, 10.0f, playerCollidingMask); //C:If the raycast is true, then there is something in between the player and the movable location
    }

    private void Move()
    {
        if(validMoveLoc)
            StartCoroutine(MoveHelper());
    }

    private IEnumerator MoveHelper()
    {
        isMoving = true;
        Vector3 curr = transform.position;
        Vector3 goal = targetPos.transform.position;
        float t = 0;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(curr, goal, t/moveDuration) + new Vector3(0, moveVertCurve.Evaluate(t/moveDuration) * jumpHeight, 0);
            yield return null;
        }
        transform.position = goal;
        isMoving = false;
    }

    private void Rotate(float degrees)
    {
        StartCoroutine(RotateHelper(degrees));
    }

    private IEnumerator RotateHelper(float degrees)
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
    }

    #endregion
    
}
