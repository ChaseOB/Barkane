using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snowball : MonoBehaviour
{
    public GameObject parentSide;
    public Transform center;
    public bool playerContact = false; //set to true when player bumps to start animation
    public LayerMask snowballCollidingMask;
    public LayerMask validLocMask;
    public float moveDuration = 0.25f;



    private void Start() {
        FindClosestFace();
    }


    public void OnPlayerCollide()
    {
        playerContact = true;
    }

    private void FindClosestFace()
    {
        RaycastHit[] hits = Physics.RaycastAll(center.position, transform.up * -1, 2, validLocMask, QueryTriggerInteraction.Collide);
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
            }
        }


        Debug.LogError("No face found for snowball");
    }


    //returns true if you can push the snowball in the given (world space) direction
    public bool CheckIfCanPushSnowball(Vector3 direction)
    {
        Vector3 convertedDir = direction.normalized * 2;

        //1. Raycast in direction to make sure square is not blocked
        bool hit = Physics.Raycast(center.position, direction, 2, snowballCollidingMask); 
        if(hit)
            return false;

        //2. Check that there is a valid square to move to
        Collider[] colliders = Physics.OverlapBox(transform.position + convertedDir, new Vector3(0.1f, 0.1f, 0.1f), Quaternion.identity, validLocMask, QueryTriggerInteraction.Collide);
        if (colliders.Length > 0)
            return true;

        return false;
    }



    public void MoveSnowball(Vector3 direction) {
        Vector3 convertedDir = direction.normalized * 2;
        StartCoroutine(AnimateSnowball(convertedDir));
    }



    private IEnumerator AnimateSnowball(Vector3 target) {
        Vector3 current = transform.position;
        float start = Time.time;
        yield return new WaitUntil(() => playerContact);
        float end = Time.time;
        float remaining = moveDuration - start + end;
        //animate 

        float t = 0;
        while (t < moveDuration)
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
