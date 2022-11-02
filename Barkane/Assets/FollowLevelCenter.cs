using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowLevelCenter : Singleton<FollowTarget>
{
    public FoldablePaper target;
    public float speed = 1.0f;

    private void Awake() 
    {
        InitializeSingleton();
        target = FindObjectOfType<FoldablePaper>();
    }
    
    void Update()
    {
        if(!target)
            target = FindObjectOfType<FoldablePaper>();
        transform.position = Vector3.Lerp(transform.position, target.centerPos, Time.deltaTime * speed);
    }
}
