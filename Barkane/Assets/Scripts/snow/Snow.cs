using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Snow : MonoBehaviour
{
    //detect whether the movement is valid.
    private bool meetSnow = false;
    private bool validMoveSnow = false;

    [SerializeField] private GameObject targetPos;
    [SerializeField] private Transform raycastStart;
    [SerializeField] private GameObject marmalade;

    private bool isMoving = false;

    private Vector2 move;
 

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("snow");
    }
    public void OnMove(InputValue value)
    {
        Vector2 move = value.Get<Vector2>();
        Debug.Log(move.y);
        if(move.y > 0.5)
             MoveSnow();

    }

    public void ValidSnowBall(bool value)
    {
        meetSnow = value; 
    }
    public void ValidMoveSnow(bool value){
        validMoveSnow = value;
    }

    public bool CheckValidSnow()
    {
        return meetSnow && validMoveSnow;
    }

    private void MoveSnow()
    {
        if(CheckValidSnow())
            StartCoroutine(MoveHelperSnow());
    }

    private IEnumerator MoveHelperSnow()
    {
        isMoving = true;
        Vector3 curr = transform.position;
        Vector3 goal = targetPos.transform.position;
        transform.position = goal;
        isMoving = false;
        yield return null;
    }

}
