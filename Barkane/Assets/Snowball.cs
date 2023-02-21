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
    public float moveDuration = 0.25f;
    private Vector3 target;



    private void Start() {
        FindClosestFace();
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
                
                this.transform.parent = parentSide.transform;
                return;
            }
        }
        Debug.LogError("No face found for snowball");
    }


    //returns true if you can push the snowball in the given (world space) direction
    public bool CheckIfCanPushSnowball(Vector3 direction)
    {   

        Vector3 convertedDir = direction.normalized * 2;
        
        print(center.position);
        print(convertedDir);


        //1. Raycast in direction to make sure square is not blocked
        if(Physics.Raycast(center.position, direction, 2, snowballCollidingMask)) 
            return false;
        if(Physics.Raycast(altRaycast.position, direction, 2, snowballCollidingMask)) 
            return false;

        //2. Check that there is a valid square to move to
        Collider[] colliders = Physics.OverlapBox(transform.position + convertedDir, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, validLocMask, QueryTriggerInteraction.Collide);
        if (colliders.Length > 0) {
            target = transform.position + convertedDir;
            Debug.DrawRay(transform.position, convertedDir, Color.magenta, 30);
            return true;
        }
        Debug.DrawRay(transform.position, convertedDir, Color.red, 30);
        return false;
    }



    public void MoveSnowball() {
        StartCoroutine(AnimateSnowball());
    }



    private IEnumerator AnimateSnowball() {
        Vector3 current = transform.position;
        float start = Time.time;
        yield return new WaitUntil(() => playerContact);
        float end = Time.time;
        float remaining = moveDuration - start + end;
        //animate 

        float t = 0;
        while (t < remaining)
        {
            t += Time.deltaTime;
            transform.position = Vector3.Lerp(current, target, t/remaining);
            yield return null;
        }
        transform.position = target;
        playerContact = false;
        //update parent

       FindClosestFace();
    }
}
