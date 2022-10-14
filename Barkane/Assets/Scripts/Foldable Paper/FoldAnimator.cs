using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldAnimator : MonoBehaviour
{
    public float foldDuration = 0.25f;
    public bool isFolding = false;
    //public bool isChecking = false;
    public FoldablePaper foldablePaper;
    public GameObject SquareCollider;

    public int foldCount = 0;
    private int internalCount = 0; //C: ticks more often than foldCount, used for priority in rendering squares

    private void Start() 
    {
        foldablePaper = FindObjectOfType<FoldablePaper>();
    }
    //C: Tries to fold the given objects. Returns true and folds if successful, returns false if this fold is not possible.
    public bool TryFold(PaperJoint foldJoint, FoldObjects foldObjects, Vector3 center, Vector3 axis, float degrees)
    {
        if(CheckCanFold(foldJoint, foldObjects, center, axis, degrees))
        {
            Fold(foldJoint, foldObjects, center, axis, degrees);
            return true;
        }
        return false;
        
    }

    public bool CheckCanFold(PaperJoint foldJoint, FoldObjects foldObjects, Vector3 center, Vector3 axis, float degrees)
    {
        if(isFolding) return false;

        //C: duplicate square hitboxes, check if they collide with any obsticles

        foreach(GameObject go in foldObjects.foldSquares)
        {
            Instantiate(SquareCollider, go.transform.position, go.transform.rotation);
        }

        return true;
    }

    //C: folds the given list of squares along the given line by the given number of degrees
    public void Fold(PaperJoint foldJoint, FoldObjects foldObjects, Vector3 center, Vector3 axis, float degrees)
    {
        if(!isFolding) 
        {
            var foldJointRenderer = foldJoint.JointRenderer;
            if(foldJointRenderer != null)
          //  DetermineVisibleSides(objectsToFold, center, axis, degrees);
                StartCoroutine(FoldHelper(foldObjects, center, axis, degrees, foldJointRenderer.DisableMeshAction, foldJointRenderer.EnableMeshAction));
            else
                StartCoroutine(FoldHelper(foldObjects, center, axis, degrees));
        }
            
    }

    /*public List<List<PaperSquare>> FindOverlappingSquares()
    {
        List<List<PaperSquare>> overlapList = new List<List<PaperSquare>>();

        Dictionary<Vector3, List<PaperSquare>> dict = new Dictionary<Vector3, List<PaperSquare>>();

        foreach(PaperSquare ps in foldablePaper.PaperSquares) {
            if(dict.ContainsKey(ps.transform.position))
            {
                dict[ps.transform.position].Add(ps);
            }
            else
            {
                List<PaperSquare> list = new List<PaperSquare>();
                list.Add(ps);
                dict.Add(ps.transform.position, list);
            }
        }

        foreach (List<PaperSquare> list in dict.Values){
            if(list.Count > 1)
                overlapList.Add(list);
        }
        return overlapList;
    }*/

















    private void DetermineVisibleSides(List<GameObject> objectsToFold, Vector3 center, Vector3 axis, float degrees)
    {
        /*check sorting:

            1. See if anything will leave its current stack
                1a. if so, remove it from the stack and update stack visuals
            2. See if anything will collide with another square
                2a. if so, see if either square is in a stack
                2b. if neither square is in a stack, create a new stack
                2c. if one square is in a stack, add the other square to that stack
                2d. if both squares are in stacks, merge the stacks together 




            1. pre move
            2. check overlapping squares and joints
            3. move pre move back to get vectors
            4. determine top/bottom based on vectors
            5. pass this info along, change accordingly when move is done. 
            */
       // GameObject tempObj = new GameObject();
       // GameObject target = new GameObject();
       // tempObj.transform.position = center;
      // target.transform.position = center;

        //Dictionary<GameObject, GameObject> parents = new Dictionary<GameObject, GameObject>();
        //Dictionary<PaperSquare, Vector3Int> targetLocs = new Dictionary<PaperSquare, Vector3Int>();
       /* foreach(GameObject o in objectsToFold)
        {
            if(o.GetComponent<PaperSquare>())
            {
                PaperSquare square = o.GetComponent<PaperSquare>();
                //Check if this square is currently in a stack. If it is, remove it
                foldablePaper.TryRemoveSquare(square);

                GameObject go = new GameObject();
                go.transform.position = o.transform.position;
                go.transform.RotateAround(center, axis, degrees);
                foreach(PaperSquare ps in foldablePaper.PaperSquares)
                {
                    if(Vector3.Magnitude(ps.transform.position - go.transform.position) < 0.01f)
                    {
                        //ps is square being collided with 
                        PaperSquareStack stack = foldablePaper.GetStackWith(ps);
                        if(stack == null)
                            stack = foldablePaper.GetStackWith(square);
                        //see if there is an existing stack that this can be added to. If not, make a new stack 
                        Debug.Log(ps.gameObject.name);
                    }
                }
            }
        }*/
       // target.transform.RotateAround(center, axis, degrees);
       // tempObj.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);

    }
    private IEnumerator FoldHelper(FoldObjects objectsToFold, Vector3 center, Vector3 axis, float degrees, System.Action beforeFold = null, System.Action afterFold = null)
    {
        isFolding = true;
        GameObject tempObj = new GameObject(); //used for reparenting/rotating
        GameObject target = new GameObject(); //used for setting correct position due to float jank
        tempObj.transform.position = center;
        target.transform.position = center;
       
        foreach(GameObject o in objectsToFold.foldSquares)
        {
            o.transform.parent = tempObj.transform;
        }
        
        foreach(GameObject o in objectsToFold.foldJoints)
        {
            o.transform.parent = tempObj.transform;
            o.GetComponent<PaperJoint>().ToggleCollider(false);
        }

        if(beforeFold != null)
            beforeFold();

        float t = 0;
        bool first = true;
        while (t < foldDuration)
        {
            t += Time.deltaTime;
            tempObj.transform.RotateAround(center, axis, (degrees / foldDuration) * Time.deltaTime);
            if(first){
                first = false;
                UpdateSquareVisibility(objectsToFold);
            }
            yield return null;
        }
        target.transform.RotateAround(center, axis, degrees);
        tempObj.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);

        foreach(GameObject o in objectsToFold.foldSquares)
        {
            o.transform.position = Vector3Int.RoundToInt(o.transform.position);
            o.transform.parent =  objectsToFold.squareParent;
        }

        foreach(GameObject o in objectsToFold.foldJoints)
        {
            o.transform.position = Vector3Int.RoundToInt(o.transform.position);
            o.transform.parent =  objectsToFold.jointParent;
            o.GetComponent<PaperJoint>().ToggleCollider(true);
        }
        Destroy(tempObj);
        Destroy(target);
        isFolding = false;

        UpdateSquareVisibility(objectsToFold);

        if(afterFold != null)
             afterFold();
        UIManager.UpdateFoldCount(++foldCount);
    }

    private void UpdateSquareVisibility(FoldObjects foldObjects){
        //update priority
        foldObjects.UpdateSquarePriority(++internalCount);
        List<List<PaperSquare>> overlaps = foldablePaper.FindOverlappingSquares();
        foreach(List<PaperSquare> list in overlaps)
        {
            Debug.Log("Overlapping Squares");
            //in each list of overlaps, we need to calculate the highest priorty top square and highest priority bottom square, then hide everything else
            List<SquareSide> topHalfList = new List<SquareSide>();
            List<SquareSide> botHalfList = new List<SquareSide>();

            //C: We arbitrarily pick one side of the first square to be the "top", then add to this list based on the normals of the other squares top/bottoms
            PaperSquare square1 = list[0];
            Vector3 topHalfNorm = square1.TopHalf.transform.up;
            //Vector3 botHalfNorm = square1.BottomHalf.transform.up;
            foreach (PaperSquare square in list){
                if(CoordUtils.ApproxSameVector(topHalfNorm, square.TopHalf.transform.up))
                {
                    topHalfList.Add(square.topSide);
                    botHalfList.Add(square.bottomSide);
                }
                else
                {
                    botHalfList.Add(square.topSide);
                    topHalfList.Add(square.bottomSide);
                }
                //Vector3 topHalfNorm = square.TopHalf.transform.up;
                //Vector3 botHalfNorm = square.BottomHalf.transform.up;
                //Debug.Log($"{square.gameObject.name} top vector {topHalfNorm} bottom vector {botHalfNorm}");
           }

            topHalfList.Sort();
            botHalfList.Sort();
            Debug.Log(topHalfList);
            Debug.Log(botHalfList);
            for (int i = 0; i < topHalfList.Count; i++)
                topHalfList[i].ToggleMesh(i == topHalfList.Count - 1);
            for (int i = 0; i < botHalfList.Count; i++)
                botHalfList[i].ToggleMesh(i == botHalfList.Count - 1);
        }
    }


}
