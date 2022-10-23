using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : Singleton<FollowTarget>
{
    public Transform target;
    public float speed = 1.0f;

    private void Awake() 
    {
        InitializeSingleton();
    }
    
    void Update()
    {
        if(target)
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * speed);
    }

    public void SetTargetAndPosition(Transform t)
    {
        target = t;
        transform.position = target.position;
    }
}
