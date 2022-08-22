using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldAnimator : MonoBehaviour
{
    public float foldDuration = 0.25f;

    //C: folds the given list of squares along the given line by the given number of degrees
    public void Fold(List<PaperSqaure> squaresToFold, Line foldLine, float degrees)
    {
        StartCoroutine(FoldHelper(squaresToFold, foldLine, degrees));
    }

    IEnumerator FoldHelper(List<PaperSqaure> squaresToFold, Line foldLine, float degrees)
    {
        GameObject tempObj = new GameObject();
        GameObject target = new GameObject();
        tempObj.transform.position = foldLine.GetCenter();
        target.transform.position = foldLine.GetCenter();
        Dictionary<PaperSqaure, GameObject> parents = new Dictionary<PaperSqaure, GameObject>();
        Dictionary<PaperSqaure, Vector3Int> targetLocs = new Dictionary<PaperSqaure, Vector3Int>();
        foreach(PaperSqaure s in squaresToFold)
        {
            parents[s] = s.transform.parent.gameObject;
            targetLocs[s] = s.targetLocation;
            s.transform.parent = tempObj.transform;
        }

        float t = 0;
        while (t < foldDuration)
        {
            t += Time.deltaTime;
            tempObj.transform.RotateAround(foldLine.p1, foldLine.p1 - foldLine.p2, (degrees / foldDuration) * Time.deltaTime);
            yield return null;
        }
        target.transform.RotateAround(foldLine.p1, foldLine.p1 - foldLine.p2, degrees);
        tempObj.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);

        foreach(PaperSqaure s in squaresToFold)
        {
            s.transform.position = Vector3Int.RoundToInt(s.transform.position);
            s.transform.parent =  parents[s].transform;
        }
        Destroy(tempObj);
        Destroy(target);
    }
}
