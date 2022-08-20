using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldablePaper : MonoBehaviour
{
    [SerializeField] private PaperSqaure[] sqaures;
    public List<PaperSqaure> fold = new List<PaperSqaure>();
    public Line foldLine;

    private void Awake() 
    {
        sqaures = GetComponentsInChildren<PaperSqaure>();    
    }

    public void TestFold()
    {
        Fold(fold, foldLine, 90);
    }

    //C: folds the given list of squares along the given line by the given number of degrees
    public void Fold(List<PaperSqaure> squaresToFold, Line foldLine, float degrees)
    {
        GameObject tempObj = new GameObject();
        tempObj.transform.position = foldLine.GetCenter();
        Dictionary<PaperSqaure, GameObject> parents = new Dictionary<PaperSqaure, GameObject>();
        foreach(PaperSqaure s in squaresToFold)
        {
            parents[s] = s.transform.parent.gameObject;
            s.transform.parent = tempObj.transform;
        }
        tempObj.transform.RotateAround(foldLine.p1, foldLine.p1 - foldLine.p2, degrees);
        foreach(PaperSqaure s in squaresToFold)
        {
            s.transform.parent =  parents[s].transform;
        }
        Destroy(tempObj);
    }
}
