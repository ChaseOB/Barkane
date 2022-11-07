using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FaceType
{
    WALKABLE,
    UNWALKABLE,
}

public class PaperSquareFace : MonoBehaviour
{
    [SerializeField] FaceType faceType;
    [SerializeField] private BoxCollider playerWalk;
    [SerializeField] private SquareSide squareSide;
    public Material walkMat;
    public Material unWalkMat; 
    private Theme theme;

    public void UpdateTheme(Theme t)
    {
        if(t != theme) {
            theme = t;
        }
        walkMat = theme.WalkMat;
        unWalkMat = theme.UnWalkMat;
        ChangeFaceType(faceType);
    }

    public void ChangeFaceType(FaceType faceType)
    {
        this.faceType = faceType;
        if(faceType == FaceType.WALKABLE)
        {
            playerWalk.enabled = true;
            squareSide.materialPrototype = walkMat;
            squareSide.UpdateMesh();
        }
        else if(faceType == FaceType.UNWALKABLE)
        {
            playerWalk.enabled = false;
            squareSide.materialPrototype = unWalkMat;
            squareSide.UpdateMesh();        
        }
        Debug.Log($"Type of {gameObject.name} changed to {faceType}");
    }
}
