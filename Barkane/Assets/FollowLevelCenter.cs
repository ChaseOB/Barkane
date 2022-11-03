using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLevelCenter : MonoBehaviour
{
    public FoldablePaper target;
    public float speed = 0.2f;

    private void Awake() 
    {
        target = FindObjectOfType<FoldablePaper>();
    }
    
    void Update()
    {
        if(!target)
            target = FindObjectOfType<FoldablePaper>();
        Vector3 targetLoc = target.centerPos - new Vector3(0, target.centerPos.y, 0);
        transform.position = Vector3.Lerp(transform.position, targetLoc, Time.deltaTime * speed);
    }
}
