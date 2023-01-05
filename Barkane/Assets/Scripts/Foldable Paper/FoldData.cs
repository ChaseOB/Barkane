using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldData: Action 
{
    public List<PaperJoint> axisJoints; //all joints along axis
    public FoldObjects foldObjects;
    public FoldObjects playerFoldObjects;
    public Vector3 center;
    public Vector3 axis;
    public float degrees;

    public FoldData() {}

    public FoldData(List<PaperJoint> aj, FoldObjects fo, FoldObjects pfo, Vector3 c, Vector3 a, float deg) {
        axisJoints = aj;
        foldObjects = fo;
        playerFoldObjects = fo;
        center = c;
        axis = a;
        degrees = deg;
    }

    public override Action GetInverse()
    {
        FoldData fd = (FoldData)this.MemberwiseClone();
        fd.degrees *= -1;
        return (Action)fd;
    }

    public override void ExecuteAction(bool undo)
    {
       GameObject.FindObjectOfType<FoldAnimator>().Fold(this, true, undo);
    }

    public List<List<PaperSquare>> FindOverlappingSquares()
    {
        List<List<PaperSquare>> overlapList = new List<List<PaperSquare>>();

        Dictionary<Vector3, List<PaperSquare>> dict = new Dictionary<Vector3, List<PaperSquare>>();

        List<PaperSquare> paperSquares = new List<PaperSquare>();
        foreach(GameObject go in foldObjects.foldSquares)
            paperSquares.Add(go.GetComponent<PaperSquare>());
        foreach(GameObject go in playerFoldObjects.foldSquares)
            paperSquares.Add(go.GetComponent<PaperSquare>());
            
        foreach(PaperSquare ps in paperSquares) {
            bool didAdd = false;
            foreach(Vector3 key in dict.Keys){
                if (Vector3.Magnitude(key - ps.transform.position) < 0.0001f) {
                    dict[key].Add(ps);
                    didAdd = true;
                }
            }
            if(!didAdd)
            {
                List<PaperSquare> list = new List<PaperSquare>();
                list.Add(ps);
                dict.Add(ps.transform.position, list);
            }
        }

        foreach (List<PaperSquare> list in dict.Values){
                overlapList.Add(list);
        }
        return overlapList;
    }
}
