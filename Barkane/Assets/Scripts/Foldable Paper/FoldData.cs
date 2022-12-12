using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldData: Action 
{
    public PaperJoint foldJoint; //Specific Joint being folded
    public List<PaperJoint> axisJoints; //all joints along axis
    public FoldObjects foldObjects;
    public FoldObjects playerFoldObjects;
    public Vector3 center;
    public Vector3 axis;
    public float degrees;

    public FoldData() {}

    public FoldData(PaperJoint fj, List<PaperJoint> aj, FoldObjects fo, FoldObjects pfo, Vector3 c, Vector3 a, float deg) {
        foldJoint = fj;
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
}
