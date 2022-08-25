using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldAnimator : MonoBehaviour
{
    public float foldDuration = 0.25f;
    public bool isFolding = false;

    //C: folds the given list of squares along the given line by the given number of degrees
    public void Fold(List<GameObject> objectsToFold, Line foldLine, float degrees)
    {
        if(!isFolding) 
            StartCoroutine(FoldHelper(objectsToFold, foldLine, degrees));
    }

    IEnumerator FoldHelper(List<GameObject> objectsToFold, Line foldLine, float degrees)
    {
        isFolding = true;
        GameObject tempObj = new GameObject();
        GameObject target = new GameObject();
        tempObj.transform.position = foldLine.GetCenter();
        target.transform.position = foldLine.GetCenter();
        Dictionary<GameObject, GameObject> parents = new Dictionary<GameObject, GameObject>();
        //Dictionary<PaperSqaure, Vector3Int> targetLocs = new Dictionary<PaperSqaure, Vector3Int>();
        foreach(GameObject o in objectsToFold)
        {
            parents[o] = o.transform.parent.gameObject;
           // targetLocs[s] = s.targetLocation;
            o.transform.parent = tempObj.transform;
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

        foreach(GameObject o in objectsToFold)
        {
            o.transform.position = Vector3Int.RoundToInt(o.transform.position);
            o.transform.parent =  parents[o].transform;
        }
        Destroy(tempObj);
        Destroy(target);
        isFolding = false;
    }
}
