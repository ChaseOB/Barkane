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
    [SerializeField] private GameObject marmalade;

    private bool isMoving = false;
    private bool validMoveLoc = false; //true if the tile in front of the player is a valid move location
    private Vector2 move;
 
    public AnimationCurve moveVertCurve;
    public float bounceHeight = 0.2f; 
    private float marmaladeY;

    private bool Snowball = false;
    public LayerMask playerCollidingMask;

    private void Start() 
    {
        marmaladeY = marmalade.transform.position.y;    
    }

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

    public bool CheckValidMove()
    {
        return validMoveLoc && 
                !Physics.Raycast(raycastStart.position, 
                                (targetPos.transform.position + new Vector3(0, 0.05f ,0) - raycastStart.position), 
                                3.0f, playerCollidingMask); 
        //C:If the raycast is true, then there is something in between the player and the movable location
    }

    private void Move()
    {
        if(CheckValidMove())
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
            transform.position = Vector3.Lerp(curr, goal, t/moveDuration);
            marmalade.transform.position = new Vector3(marmalade.transform.position.x, 
                                            marmaladeY + moveVertCurve.Evaluate(t/moveDuration) * bounceHeight,  
                                            marmalade.transform.position.z);
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
    private void meetSnowBall(bool value){
        Snowball = value;
    }

    #endregion
    
}
