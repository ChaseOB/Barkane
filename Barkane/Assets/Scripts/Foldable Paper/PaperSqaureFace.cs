using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FaceType
{
    WALKABLE,
    UNWALKABLE,
}

public class PaperSqaureFace : MonoBehaviour
{
    [SerializeField] FaceType faceType;

    public void ChangeFaceType(FaceType faceType)
    {
        this.faceType = faceType;

        //Do things to change the type
        Debug.Log($"Type of {gameObject.name} changed to {faceType}");
    }
}
