using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldChecker : MonoBehaviour
{
    //This class checks the validity of a given fold


    //Checks if the given fold data is a valid fold. If so, returns true. If not, returns
    //false
    public bool CheckFoldBool(FoldData foldData)
    {
        FoldFailureType failureType = CheckFold(foldData);
        if(failureType == FoldFailureType.NONE)
            return true;
        return false;
    }

    /*Checks if the given fold data is a valid fold. Returns an enum with the fold failure type
    NONE        Valid fold
    KINKED      Could not physically fold along line
    PAPERCLIP   folding paper through itself
    COLLISION   hit object, player, paper, etc on fold path
    NOCHECK     could not check fold due to another action occurring 
    */
    public FoldFailureType CheckFold(FoldData foldData) 
    {
        if(!CheckKinkedJoint(foldData.axisJoints))
            return FoldFailureType.KINKED;

        return FoldFailureType.NONE;
    }

     /* FOLD CHECK 1: KINKED JOINTS
            This checks that the selected joint lies within a single plane. If it does, this might be a valid fold. If not, 
            This is not a valid fold and we return false

            KNOWN ISSUES: None
        */
    public bool CheckKinkedJoint(List<PaperJoint> joints)
    {
       
        HashSet<int> x = new HashSet<int>();
        HashSet<int> y = new HashSet<int>();
        HashSet<int> z = new HashSet<int>();

        foreach(PaperJoint pj in joints)
        {
            x.Add(Vector3Int.RoundToInt(pj.transform.position).x);
            y.Add(Vector3Int.RoundToInt(pj.transform.position).y);
            z.Add(Vector3Int.RoundToInt(pj.transform.position).z);
        }

        if((x.Count > 1 && y.Count > 1) || (x.Count > 1 && z.Count > 1) || (z.Count > 1 && y.Count > 1)) {
            Debug.Log($"Cannot fold: joint is kinked. {x.Count} {y.Count} {z.Count}");
            return false;
        }
        return true;
    }



}


public enum FoldFailureType {
    NONE, //Valid fold
    KINKED, //Could not physically fold along line
    PAPERCLIP, //folding paper through itself
    COLLISION, //hit object, player, paper, etc on fold path
    NOCHECK, //could not check fold due to another action occurring 
}
