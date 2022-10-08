using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldAnimator : MonoBehaviour
{
    public float foldDuration = 0.25f;
    public bool isFolding = false;
    public FoldablePaper foldablePaper;


    private void Start() 
    {
        foldablePaper = FindObjectOfType<FoldablePaper>();
    }
    //C: Tries to fold the given objects. Returns true and folds if successful, returns false if this fold is not possible.
   /* public bool TryFold(List<GameObject> objectsToFold, Line foldLine, float degrees)
    {
        if(CheckCanFold(objectsToFold, foldLine, degrees))
        {
            Fold(objectsToFold, foldLine, degrees);
            return true;
        }
        return false;
        
    }

    public bool CheckCanFold(List<GameObject> objectsToFold, Line foldLine, float degrees)
    {
        if(isFolding) return false;


        return true;
    }*/

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
        //Dictionary<PaperSqaure, Vector3Int> targetLocs = new Dictionary<PaperSqaure, Vector3Int>();
       /* foreach(GameObject o in objectsToFold)
        {
            if(o.GetComponent<PaperSqaure>())
            {
                PaperSqaure square = o.GetComponent<PaperSqaure>();
                //Check if this square is currently in a stack. If it is, remove it
                foldablePaper.TryRemoveSquare(square);

                GameObject go = new GameObject();
                go.transform.position = o.transform.position;
                go.transform.RotateAround(center, axis, degrees);
                foreach(PaperSqaure ps in foldablePaper.PaperSqaures)
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
        while (t < foldDuration)
        {
            t += Time.deltaTime;
            tempObj.transform.RotateAround(center, axis, (degrees / foldDuration) * Time.deltaTime);
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

        if(afterFold != null)
             afterFold();
    }


}
