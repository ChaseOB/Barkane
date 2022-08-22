using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldablePaper : MonoBehaviour
{
    [SerializeField] private PaperSqaure[] sqaures;
    public List<GameObject> fold = new List<GameObject>();
    public Line foldLine;
    public FoldAnimator foldAnimator;

    private void Awake() 
    {
        sqaures = GetComponentsInChildren<PaperSqaure>();    
    }

    public void TestFold()
    {
        foldAnimator.Fold(fold, foldLine, 90);
    }
}
