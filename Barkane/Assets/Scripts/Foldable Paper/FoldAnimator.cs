using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing.Imaging;
using Unity.Mathematics;
using UnityEngine;

public class FoldAnimator : MonoBehaviour
{
    // public float foldDuration = 0.25f;
    private bool isFolding = false;
    public bool IsFolding => isFolding;
    // //public bool isChecking = false;
    public FoldablePaper foldablePaper;
    // public GameObject SquareCollider;

    // public int foldCount = 0;

    // public LayerMask squareCollidingMask;

    // public bool checkRaycast = false; //C: set to true when the rest of the check is good
    // public bool raycastCheckDone = false;
    // public FoldData foldData = new FoldData();
    // public Coroutine checkCoroutine = null;
    // public bool raycastCheckReturn = false;
    // public bool crDone = false;



    private void Start() 
    {
        foldablePaper = FindObjectOfType<FoldablePaper>();
        PaperStateManager.Instance.foldAnimator = this;
    }

    public void Fold(FoldData fd, PaperState state, ActionCallEnum actionCallEnum)
    {
        if(isFolding) return;
        isFolding = true;

        if(!ActionLockManager.Instance.TryTakeLock(this)){
                Debug.LogError("Action Lock taken, can't fold (this is bad)");
                return;
            }
       // OnFold?.Invoke(this, new FoldArgs{fd = fd});
        StartCoroutine(AnimateFold(fd, state, actionCallEnum));
        //callback?.Invoke();
    }

     private IEnumerator AnimateFold(FoldData fd, PaperState state, ActionCallEnum actionCallEnum)
     {
        //OnFold?.Invoke(this, new FoldArgs{fd = fd});

        List<FoldableObject> objectsToFold = fd.foldObjects;
        Vector3 center = fd.axisPosition;

        AudioManager.Instance?.Play("Fold");
        isFolding = true;
        GameObject tempObj = new GameObject(); //used for reparenting/rotating
        tempObj.transform.position = center;

        // foreach (GameObject o in objectsToFold.foldSquares)
        // {
        //     o.transform.SetParent(tempObj.transform, worldPositionStays: true);
        // }
        
        // foreach(GameObject o in objectsToFold.foldJoints)
        // {
        //     o.transform.SetParent(tempObj.transform, worldPositionStays: true);
        //     o.GetComponent<PaperJoint>().ToggleCollider(false);
        // }

        List<SquareData> squares = new();
        foreach(FoldableObject f in objectsToFold)
        {
            f.SetParent(tempObj.transform);
        }

        float t = 0;
        // foreach(PaperJoint pj in foldablePaper.PaperJoints)
        //     pj.OnFold();

        float foldDuration = 0.25f;
        while (t < foldDuration)
        {
            t += Time.deltaTime;
            tempObj.transform.SetPositionAndRotation(center, Quaternion.AngleAxis(90 * t / foldDuration, fd.axisVector));
            foreach(SquareData s in foldablePaper.squareData)
            {
                s.paperSquare.YOffset = math.lerp(s.currentYOffset, s.targetYOffset, t / foldDuration);
            }
            yield return null;
        }

        tempObj.transform.SetPositionAndRotation(center, Quaternion.AngleAxis(90, fd.axisVector));

        isFolding = false;
        
        foreach(FoldableObject f in objectsToFold)
        {
            f.SetParent(null);
        }
        Destroy(tempObj);

        SetFoldPosition(state);
        isFolding = false;
        ActionLockManager.Instance.TryRemoveLock(this);
        TileSelector.Instance.state = SelectState.NONE;
    }

    private void SetFoldPosition(PaperState state)
    {
        state.SendToTarget();
        PaperStateManager.Instance.SetPaperState(state);
    }

//     public void Fold(FoldData fd, bool fromStack = false, bool undo = false)
//     {
//         if(!isFolding) 
//         {
//             var foldJointRenderer = fd.axisJoints[0].JointRenderer;
//             if(!ActionLockManager.Instance.TryTakeLock(this)){
//                 Debug.LogError("Action Lock taken, can't fold (this is bad)");
//                 return;
//             }
//             if(foldJointRenderer != null)
//                 StartCoroutine(FoldHelper(fd, fromStack, undo, fd.foldObjects.DisableJointMeshes, fd.foldObjects.EnableJointMeshes));
//             else
//                 StartCoroutine(FoldHelper(fd, fromStack, undo));
//         }
            
//     }

//     private IEnumerator FoldHelper(FoldData fd, bool fromStack = false, bool undo = false, System.Action beforeFold = null, System.Action afterFold = null)
//     {
//         OnFold?.Invoke(this, new FoldArgs{fd = fd});

//         FoldObjects objectsToFold = fd.foldObjects;
//         Vector3 center = fd.center;

//         AudioManager.Instance?.Play("Fold");
//         isFolding = true;
//         // Debug.DrawRay(center, fd.axis, Color.black, 3);
//         GameObject tempObj = new GameObject(); //used for reparenting/rotating
//         GameObject target = new GameObject(); //used for setting correct position due to float jank
//         tempObj.transform.position = center;
//         target.transform.position = center;

//         //GameObject localSpaceRootDebug = new GameObject();
//         //localSpaceRootDebug.transform.SetParent(tempObj.transform);
//         //localSpaceRootDebug.transform.position = Vector3.zero;
//         //localSpaceRootDebug.transform.rotation = Quaternion.identity;

//         // Records the initial transform matrices
//         // This way tile coordinates are evaluated normally within local space
//         // Because Occlusion uses coordinate MOD 2 to determine orientation, the local
//         // space anchored around "center" actually messes everything up
//         var temp2Local = tempObj.transform.localToWorldMatrix;
//         var local2Temp = tempObj.transform.worldToLocalMatrix;

//         foreach (GameObject o in objectsToFold.foldSquares)
//         {
//             o.transform.SetParent(tempObj.transform, worldPositionStays: true);
//         }
        
//         foreach(GameObject o in objectsToFold.foldJoints)
//         {
//             o.transform.SetParent(tempObj.transform, worldPositionStays: true);
//             o.GetComponent<PaperJoint>().ToggleCollider(false);
//         }

//         if(beforeFold != null)
//             beforeFold();

//         StoreAllSquarePos();
//         float t = 0;
//         int wait = 1;
//         foreach(PaperJoint pj in foldablePaper.PaperJoints)
//             pj.OnFold();

//         (Matrix4x4 encode, Matrix4x4 decode) replay(float t)
//         {
//             // note that local space root takes the inverse of tempObj at t=0
//             var temp2Wld = Matrix4x4.TRS(center, Quaternion.AngleAxis(fd.degrees * t, fd.axis), Vector3.one);
//             var wld2Temp = temp2Wld.inverse;

//             var encode = temp2Local * wld2Temp;
//             var decode = temp2Wld * local2Temp;

//             return (encode, decode);
//         }
//         var (encode0, decode0) = replay(0);
//         fd.foldObjects.TransferToLocalOcclusionMap(encode0, decode0);
//         foldablePaper.OcclusionMap.Prune();

//         while (t < foldDuration)
//         {
//             t += Time.deltaTime;
//             tempObj.transform.SetPositionAndRotation(center, Quaternion.AngleAxis(fd.degrees * t / foldDuration, fd.axis));
//             wait--;

//             //Debug.Log($"Replay w2l --\n{localSpaceRootDebug.transform.worldToLocalMatrix} --\n {replay(t / foldDuration).encode}");
//             //Debug.Log($"Replay l2w --\n{localSpaceRootDebug.transform.localToWorldMatrix} --\n {replay(t / foldDuration).decode}");
//             if (wait == 0){
//                 // UpdateSquareVisibility(objectsToFold);
//             }
//             yield return null;
//         }

//         target.transform.RotateAround(center, fd.axis, fd.degrees);
//         tempObj.transform.SetPositionAndRotation(target.transform.position, target.transform.rotation);

//         foreach (GameObject o in objectsToFold.foldSquares)
//         {
//             o.transform.position = Vector3Int.RoundToInt(o.transform.position);
//         }

//         foreach(GameObject o in objectsToFold.foldJoints)
//         {
//             o.transform.position = Vector3Int.RoundToInt(o.transform.position);
//         }

//         fd.foldObjects.MergeWithGlobalOcclusionMap(foldablePaper.OcclusionMap, center, fd.degrees > 0 ? fd.axis : -fd.axis, replay);

//         foreach (GameObject o in objectsToFold.foldSquares)
//         {
//             o.transform.SetParent(objectsToFold.squareParent, worldPositionStays: true);
//         }

//         foreach (GameObject o in objectsToFold.foldJoints)
//         {
//             o.transform.SetParent(objectsToFold.jointParent, worldPositionStays: true);
//             o.GetComponent<PaperJoint>().ToggleCollider(true);
//         }

//         isFolding = false;

//         Destroy(tempObj);
//         Destroy(target);

//         // UpdateSquareVisibility(objectsToFold);

//         if(afterFold != null)
//              afterFold();
//         if(undo)
//             foldCount--;
//         else
//             foldCount++;
//         UIManager.UpdateFoldCount(foldCount);
//         LevelManager.Instance?.SetFoldCount(foldCount);
//         if(!fromStack && !undo) {
//             UndoRedoManager.Instance?.AddAction(fd);
//         }
//         TileSelector.Instance.state = SelectState.NONE;
//         ActionLockManager.Instance.TryRemoveLock(this);
//     }

//     private void StoreAllSquarePos()
//     {
//         foreach(PaperSquare ps in foldablePaper.PaperSquares)
//         {
//             ps.StorePosition(ps.transform.position);
//         }
//     }

//     private void UpdateSquareVisibility(FoldObjects foldObjects)
//     {
//         List<List<PaperSquare>> overlaps = foldablePaper.FindOverlappingSquares();

//         foreach(PaperSquare ps in foldablePaper.PaperSquares)
//             ps.ForceRefsUpdate();
    
//         foreach(List<PaperSquare> list in overlaps)
//         {
//             if(list.Count == 1) //C: only 1 square, enable both meshes
//             {
//                 list[0].topStack = null;
//                 list[0].bottomStack = null;
//                 list[0].ToggleBottom(true);
//                 list[0].ToggleTop(true);
//             }
            
//             else
//             {
//                     //We arbitrarily pick one side of the first square to be the "top", which is then used as a comparison for other square's top/bottoms
//                 Vector3 topHalfNorm = list[0].TopHalf.transform.up;
                    
//                 HashSet<GameObject> activeSides = new HashSet<GameObject>();
//                 HashSet<GameObject> activeFoldSides = new HashSet<GameObject>();

//                 GameObject foldTop = null;
//                 GameObject foldBot = null;
//                 GameObject stationaryTop = null;
//                 GameObject stationaryBot = null;

//                 Vector3 prevPos = Vector3.zero;
//                 //C: there should be a total of 2 sides enabled in each list currently. We need to figure out which of them to now hide.
//                 foreach(PaperSquare ps in list) 
//                 {
//                     activeSides.UnionWith(ps.GetOpenSides(!CoordUtils.ApproxSameVector(topHalfNorm, ps.TopHalf.transform.up)));
//                     if(foldObjects.foldSquares.Contains(ps.gameObject))
//                     {
//                          //   Debug.Log($"{ps.gameObject.name} is in fold list on stack of size {list.Count}");
//                         activeFoldSides.UnionWith(ps.GetOpenSides(!CoordUtils.ApproxSameVector(topHalfNorm, ps.TopHalf.transform.up)));
//                         prevPos = ps.storedPos;
//                     }
//                 }
//                 foreach(GameObject go in activeSides)
//                 {
//                     if(activeFoldSides.Contains(go))
//                     {
//                             //Debug.Log(go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name + " is in active fold sides");
//                         if(CoordUtils.ApproxSameVector(topHalfNorm, go.transform.up))
//                         {
//                             foldTop = go;
//                               //  Debug.Log("fold top is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
//                         }
//                         else
//                         {
//                             foldBot = go;
//                              //   Debug.Log("fold bot is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
//                         }
//                     }
//                     else
//                     {
//                         if(CoordUtils.ApproxSameVector(topHalfNorm, go.transform.up))
//                         {
//                             stationaryTop = go;
//                                // Debug.Log("stat top is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
//                         }
//                         else
//                         {
//                             stationaryBot = go;
//                                // Debug.Log("stat bot  is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
//                         }
//                     }
//                 }
//                /* if(foldTop == null)
//                     Debug.Log("fold top is null");
//                 if(foldBot == null)
//                     Debug.Log("fold bot is null");
//                 if(stationaryTop == null)
//                     Debug.Log("stat top is null");
//                 if(stationaryBot == null)
//                     Debug.Log("stat bot is null");
// */
//                 if(foldTop != null && foldBot != null && stationaryTop != null && stationaryBot != null)
//                 {
//                     float topDist = Vector3.Magnitude(prevPos - stationaryTop.transform.position);
//                     float botDist = Vector3.Magnitude(prevPos - stationaryBot.transform.position);
//                     if(topDist == botDist) 
//                         Debug.Log("same dist");
//                     //foldTop.GetComponentInParent<PaperSquare>().invertForGetOpen = !CoordUtils.ApproxSameVector(foldTop.transform.up, foldTop.GetComponentInParent<PaperSquare>().TopHalf.transform.up);
//                     if(topDist > botDist) //new square is on the bottom, join top of fold and bottom of stationary
//                     {
//                         Debug.Log("top side");
//                         foldTop.SetActive(false);
//                         stationaryBot.SetActive(false);
//                         PaperSquare foldSquare = foldTop.GetComponentInParent<PaperSquare>();
//                         PaperSquare statSquare = stationaryBot.GetComponentInParent<PaperSquare>();
//                         foldSquare.bottomStack = statSquare;
//                         statSquare.topStack = foldSquare;
//                         foldSquare.UpdateHitboxes();
//                         statSquare.UpdateHitboxes();
//                     }
//                     else //new fold square is on the top, join bot of fold and top of stationary
//                     {
//                         Debug.Log("bottom side");
//                         foldBot.SetActive(false);
//                         stationaryTop.SetActive(false);
//                         PaperSquare foldSquare = foldBot.GetComponentInParent<PaperSquare>();
//                         PaperSquare statSquare = stationaryTop.GetComponentInParent<PaperSquare>();
//                         foldSquare.topStack = statSquare;
//                         statSquare.bottomStack = foldSquare;
//                         foldSquare.UpdateHitboxes();
//                         statSquare.UpdateHitboxes();
//                     }
//                 }
//             }
//         }
//     }

   
}


