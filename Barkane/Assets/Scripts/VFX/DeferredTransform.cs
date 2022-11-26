using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeferredTransform : MonoBehaviour
{
    [HideInInspector] public Vector3 wpos;
    [HideInInspector] public float scl = 1f;
    [HideInInspector] public Vector3 up;

    // higher, max 1 = more responsive but gittery; lower, min 0 = less responsive but smooth
    [HideInInspector] public float responsiveness;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = transform.position * (1 - responsiveness) + wpos * responsiveness;
        transform.localScale = new Vector3(1, transform.localScale.y * (1 - responsiveness) + scl * responsiveness, 1);
        transform.up = Vector3.Slerp(transform.up, up, responsiveness);
    }
}
