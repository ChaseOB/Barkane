using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Line : MonoBehaviour
{
    public Vector3 p1;
    public Vector3 p2;

    public Vector3 GetCenter()
    {
        return (p1 + p2) / 2;
    }
}
