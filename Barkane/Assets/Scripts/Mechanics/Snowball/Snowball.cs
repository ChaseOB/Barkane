using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowball : MonoBehaviour
{
    public GameObject parentSide;
    public Transform center;
    public Transform altRaycast;
    public bool playerContact = false; //set to true when player bumps to start animation
    public LayerMask snowballCollidingMask;
    public LayerMask validLocMask;
    private float moveDuration = 0.5f;
    private Vector3 target;


    public class SnowballMoveData
    {
        public Vector3 start;
        public Vector3 end;
        public float contactTime;
        public int moveNum;

        public SnowballMoveData(Vector3 start, Vector3 end, float contactTime, int moveNum)
        {
            this.start = start;
            this.end = end;
            this.contactTime = contactTime;
            this.moveNum = moveNum;
        }

        public override string ToString()
        {
            return$"Start {start} End {end} ContactTime {contactTime} MoveNum {moveNum}";
        }

    }

    public Stack<SnowballMoveData> snowballMoves = new();

    private void Start() {
        FindClosestFace();
        moveDuration = FindObjectOfType<PlayerMovement>().moveDuration;
    }


    public void OnPlayerCollide()
    {
        playerContact = true;
    }

    private void FindClosestFace()
    {
        RaycastHit[] hits = Physics.RaycastAll(center.position, transform.up * -1, 1.1f, validLocMask, QueryTriggerInteraction.Collide);
        foreach(RaycastHit hit in hits) {
            if(hit.rigidbody.GetComponent<PaperSquare>() != null) {
                PaperSquare ps = hit.rigidbody.GetComponent<PaperSquare>();
                GameObject top = ps.TopHalf;
                GameObject bottom = ps.BottomHalf;

                float topDist = Vector3.Magnitude(center.position - top.transform.position);
                float botDist = Vector3.Magnitude(center.position - bottom.transform.position);

                if(topDist < botDist)
                    parentSide = top;
                else
                    parentSide = bottom;
                
                this.transform.parent = parentSide.GetComponent<SquareSide>().visualParent == null ? parentSide.GetComponent<SquareSide>().transform : parentSide.GetComponent<SquareSide>().visualParent.transform;
                return;
            }
        }
        Debug.LogError("No face found for snowball");
    }


    //returns true if you can push the snowball in the given (world space) direction
    public bool CheckIfCanPushSnowball(Vector3 direction)
    {   
        Vector3 convertedDir = direction.normalized * 2;

        //1. Raycast in direction to make sure square is not blocked
        if(Physics.Raycast(center.position, direction, 2, snowballCollidingMask)) 
            return false;
        if(Physics.Raycast(altRaycast.position, direction, 2, snowballCollidingMask)) 
            return false;

        //2. Check that there is a valid square to move to
        Collider[] colliders = Physics.OverlapBox(transform.position + convertedDir, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, validLocMask, QueryTriggerInteraction.Collide);
        if (colliders.Length > 0) {
            target = GetTarget(direction);
            Debug.DrawRay(transform.position, convertedDir, Color.magenta, 30);
            return true;
        }
        Debug.DrawRay(transform.position, convertedDir, Color.red, 30);
        return false;
    }

    public Vector3 GetTarget(Vector3 direction)
    {
        return transform.position + 2 * direction;
    }



    public void MoveSnowball(PlayerMove move, ActionCallEnum source) {
        if(source != ActionCallEnum.UNDO)
            StartCoroutine(AnimateSnowballNormal(move));
        else
        {
            if(move.moveNum == snowballMoves.Peek().moveNum)
                StartCoroutine(AnimateSnowballReverse());
        }
    }



    private IEnumerator AnimateSnowballNormal(PlayerMove move) {
        Vector3 current = transform.position;
        float start = Time.time;

        yield return new WaitUntil(() => playerContact);

        float end = Time.time;
        float remaining = moveDuration - (end - start);

        float t = 0;
        while (t < remaining)
        {
            t += Time.deltaTime;
            if(t >= remaining) break;
            transform.position = Vector3.Lerp(current, target, t/remaining);
            yield return null;
        }
        transform.position = target;
        playerContact = false;
        
        SnowballMoveData data = new(current, target, remaining, move.moveNum);
        snowballMoves.Push(data);

        FindClosestFace();
    }

    private IEnumerator AnimateSnowballReverse()
    {
        print("reversing snowball");
        SnowballMoveData move = snowballMoves.Pop();
        Vector3 startPos = move.start;
        Vector3 endPos = move.end;
        float contactTime = move.contactTime;
        float t = 0;
        while(t < contactTime)
        {
            t += Time.deltaTime;
            if(t >= contactTime) break;
            transform.position = Vector3.Lerp(startPos, endPos, 1 - (t/contactTime));
            yield return null;
        }
        transform.position = startPos;
        playerContact = false;

        FindClosestFace();
    }
}
