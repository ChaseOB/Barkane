using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class FoldChecker : Singleton<FoldChecker>
{
    public GameObject SquareCollider;
    public LayerMask squareCollidingMask;

    private void Awake() {
        InitializeSingleton();
    }
    
    //Checks if the given fold data is a valid fold. If so, returns true. If not, returns false
    public bool CheckFoldBool(FoldData foldData, FoldablePaper foldablePaper)
    {
        FoldFailureType failureType = CheckFold(foldData, foldablePaper);
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
    public FoldFailureType CheckFold(FoldData foldData, FoldablePaper foldablePaper) 
    {
        if(!ActionLockManager.Instance.TryTakeLock(this))
            return FoldFailureType.NOCHECK;
        if(!CheckKinkedJoint(foldData.axisJoints))
        {
            ActionLockManager.Instance.TryRemoveLock(this);
            return FoldFailureType.KINKED;
        }
        if(!CheckPaperClipping(foldData, foldablePaper))
        {
            ActionLockManager.Instance.TryRemoveLock(this);
            return FoldFailureType.PAPERCLIP;
        }
        if(!CheckCollision(foldData))
        {
            ActionLockManager.Instance.TryRemoveLock(this);
            return FoldFailureType.COLLISION;
        }
        ActionLockManager.Instance.TryRemoveLock(this);
        return FoldFailureType.NONE;
    }

     /* FOLD CHECK 1: KINKED JOINTS
            This checks that the selected joint lies within a single plane. If it does, this might be a valid fold. If not, 
            This is not a valid fold and we return false

            KNOWN ISSUES: None
        */
    public static bool CheckKinkedJoint(List<PaperJoint> joints)
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

     /* FOLD CHECK 2: PAPER CLIPPING
        This checks that back to back squares do not clip through eachother. This is because the squares are so
        thin that squares at the same location will not be caught using the raycast, thus another approach is needed.


        In order to check this, we need to check the hidden sides of the current squares. If a hidden side of a fold side
        square touches a hidden side of a non-fold side square, then there is the potential for clipping (if 2 hidden
        sides touch but both are on the same side of the fold those sides will remain hidden)
    */

    private bool CheckPaperClipping(FoldData fd, FoldablePaper foldablePaper)
    {
        List<(SquareSide, SquareSide)> pairs = new List<(SquareSide, SquareSide)>();

        pairs = foldablePaper.OcclusionMap.SeparatorPairs(fd.foldObjects.SidesSet, fd.playerFoldObjects.SidesSet);
        foreach((SquareSide, SquareSide) pair in pairs) {
            string s1 = pair.Item1.GetComponentInParent<PaperSquare>().name;
            string s2 = pair.Item2.GetComponentInParent<PaperSquare>().name;
            print($"Pair: {s1} {pair.Item1.name}, {s2} {pair.Item2.name}");
        }
        //WIP: WILL ALWAYS RETURN TRUE RN
        return true;
    }


    /* FOLD CHECK 2: PAPER CLIPPING
        This checks that back to back squares do not clip through eachother. This is because the squares are so
        thin that squares at the same location will not be caught using the raycast, thus another approach is needed.

        This part of the check also updates the visibile faces of each square
        KNOWN ISSUES: 
        -pretty good with 2 squares, not so much for higher quantities 
        -sorting is straight up wrong sometimes. 
        -generally janky and unreliable, needs a total overhaul
    */

    /*private bool CheckPaperClipping(FoldData fd)
    {
        Dictionary<Vector3Int, HashSet<PaperSquare>> overlaps = fd.FindOverlappingSquares();
        foreach(HashSet<PaperSquare> list in overlaps.Values)
        {
            if(list.Count > 1) //C: if count = 1 then only 1 square, can't fold through itself
            {
                Debug.Log("Check Clip Square");
                GameObject parent = new GameObject();
                parent.transform.position = fd.center;
               // List<GameObject> activeSides = new List<GameObject>();
                List<GameObject> inactiveSides = new List<GameObject>();
                GameObject t1 = new GameObject();
                GameObject t2 = new GameObject();
                foreach(PaperSquare ps in list) 
                {
                    //if(ps.bottomSide.Visibility == SquareSide.SideVisiblity.full)
                      //  activeSides.Add(ps.BottomHalf);
                    //if(ps.topSide.Visibility == SquareSide.SideVisiblity.full)
                      //  activeSides.Add(ps.TopHalf);
                    if(ps.bottomSide.Visibility == SquareSide.SideVisiblity.none)
                        inactiveSides.Add(ps.BottomHalf);
                    if(ps.topSide.Visibility == SquareSide.SideVisiblity.none)
                        inactiveSides.Add(ps.TopHalf);
                }
               // print("Overlap Found");
           //     foreach(PaperSquare ps in list)
           //             print(ps.gameObject.name);
          //     if(activeSides.Count != 2){
           //         Debug.LogError($"!2 active sides in a single location (this should not happen). Count: {activeSides.Count}");
          //          foreach(PaperSquare ps in list)
            //            print(ps.gameObject.name);
            //    }
                for(int i=0; i < inactiveSides.Count; i++)
                {   
                    GameObject o1 = inactiveSides[i];
                    for(int j = i + 1; j < inactiveSides.Count; j++)
                    {
                        GameObject o2 = inactiveSides[j];
                        if(o1 == o2) continue;

                        GameObject o1parent = o1.GetComponentInParent<PaperSquare>().gameObject;
                        GameObject o2parent = o2.GetComponentInParent<PaperSquare>().gameObject;
                        //if true, one square is in the fold and the other is on the player side. If false, both squares are on the same side, so they cannot intersect eachother during the fold
                        bool diffGroups = (fd.foldObjects.foldSquares.Contains(o1parent)
                                            != fd.foldObjects.foldSquares.Contains(o2parent));
                        if(!diffGroups) continue;

                        //CURRENT ISSUE: Overlaps between all faces, not just direct ones
                        //Example
                        //
                        //    1 top vis
                        //    1 bot hidden
                        //    5 bot hidden
                        //    5 top hidden
                        //    4 top hidden
                        //    4 bottom vis
                        // 
                        //   I only want the pairs (1bot, 5bot) and (5top, 4bot) but i get (1bot, 5bot), (1bot, 5top), (1bot, 4bot)... etc

                        Debug.Log($"Found overlap: {o1.name} {o1parent.name}, {o2.name} {o2parent.name}");
                        //C: Else, check position of the ends of the normal vectors before and after fold
                    // if there is no clipping, then the points at the ends of the normals will be farther apart (point away)
                    // than if there was clipping (point towards eachother). So we can check this fold and the other fold direction
                    // by folding 180* after the intial fold and then comapare distances
                    
                    t1.transform.SetPositionAndRotation(o1.transform.position, o2.transform.rotation);
                    t2.transform.SetPositionAndRotation(o1.transform.position, o2.transform.rotation);        
                    if(fd.foldObjects.foldSquares.Contains(o1.GetComponentInParent<PaperSquare>().gameObject))
                        t1.transform.parent = parent.transform;
                    else
                        t2.transform.parent = parent.transform;    

                    parent.transform.RotateAround(fd.center, fd.axis, fd.degrees);
                    Vector3 t3 = t1.transform.position + t1.transform.up * 0.1f;
                    Vector3 t4 = t2.transform.position + t2.transform.up * 0.1f;
                    float d1 = Vector3.Distance(t3, t4);
                    Debug.DrawLine(t3, t4, Color.blue, 30);
                    
                    parent.transform.RotateAround(fd.center, fd.axis, 180);
                    t3 = t1.transform.position + t1.transform.up * 0.1f;
                    t4 = t2.transform.position + t2.transform.up * 0.1f;   
                    float d2 = Vector3.Distance(t3, t4);
                    Debug.DrawLine(t3, t4, Color.yellow, 30);

               
                    if(d1 > d2) {
                        Debug.Log($"Can't fold, would clip: {o1.name} {o1parent.name}, {o2.name} {o2parent.name}");
                        //Debug.Log($"Cannot fold: would clip through adj paper {o1.transform.up} {o2.transform.up}");
                        //Destroy(t1);
                       // Destroy(t2);
                       // Destroy(parent);
                       // return false;
                    }
                    }
                }*/




                                //C: if the active sides of this stack are both in or both out of the fold, then they won't clip

              /*  if(activeSides.Count == 2 &&
                    fd.foldObjects.foldSquares.Contains(activeSides[0].GetComponentInParent<PaperSquare>().gameObject)
                    != fd.foldObjects.foldSquares.Contains(activeSides[1].GetComponentInParent<PaperSquare>().gameObject))
                {
                    //C: Else, check position of the ends of the normal vectors before and after fold
                    // if there is no clipping, then the points at the ends of the normals will be farther apart (point away)
                    // than if there was clipping (point towards eachother). So we can check this fold and the other fold direction
                    // by folding 180* after the intial fold and then comapare distances
                    
                    t1.transform.SetPositionAndRotation(activeSides[0].transform.position, activeSides[0].transform.rotation);
                    t2.transform.SetPositionAndRotation(activeSides[1].transform.position, activeSides[1].transform.rotation);        
                    if(fd.foldObjects.foldSquares.Contains(activeSides[0].GetComponentInParent<PaperSquare>().gameObject))
                        t1.transform.parent = parent.transform;
                    else
                        t2.transform.parent = parent.transform;    

                    parent.transform.RotateAround(fd.center, fd.axis, fd.degrees);
                    Vector3 t3 = t1.transform.position + t1.transform.up * 0.1f;
                    Vector3 t4 = t2.transform.position + t2.transform.up * 0.1f;
                    float d1 = Vector3.Distance(t3, t4);
                    Debug.DrawLine(t3, t4, Color.blue, 30);
                    
                    parent.transform.RotateAround(fd.center, fd.axis, 180);
                    t3 = t1.transform.position + t1.transform.up * 0.1f;
                    t4 = t2.transform.position + t2.transform.up * 0.1f;   
                    float d2 = Vector3.Distance(t3, t4);
                    Debug.DrawLine(t3, t4, Color.yellow, 30);

               
                    if(d1 < d2) {
                        Debug.Log($"Cannot fold: would clip through adj paper {activeSides[0].transform.up} {activeSides[1].transform.up}");
                        Destroy(t1);
                        Destroy(t2);
                        Destroy(parent);
                        return false;
                    }
                }
                Destroy(t1);
                Destroy(t2);
                Destroy(parent);
                Destroy(t1);
                Destroy(t2);
                Destroy(parent);
            }
        }
    }*/


    /* FOLD CHECK 3: PAPER AND OBJECT COLLISION
            This checks that Paper squares do not clip through other squares or obstacles. This also checks that obstacles do not clip 
            through squares.

            KNOWN ISSUES: 
            -won't let you fold crystal into player
            -clipping squares through objects is hard, but clipping objects into squares is easy (fixed?)
        */


    private bool CheckCollision(FoldData fd) {
        Debug.Log("checking raycast...");
        //checkRaycast = false;
        int numChecks = 10;
        
        GameObject parent2 = new GameObject("parent 2");
        parent2.transform.position = fd.center;
        List<GameObject> copiesList = new List<GameObject>();
        List<GameObject> obstList = new List<GameObject>();
        Dictionary<GameObject, Vector3> colliderDict = new Dictionary<GameObject, Vector3>();
        foreach(GameObject go in fd.foldObjects.foldSquares)
        {
            GameObject newSquare = Instantiate(SquareCollider, go.transform.position, go.transform.rotation);
            newSquare.name = "ns";
            newSquare.transform.parent = parent2.transform;
            copiesList.Add(newSquare);
            BlocksFold[] bf = go.GetComponentsInChildren<BlocksFold>();
            //if(!invertFold){
            foreach (BlocksFold bfold in bf)
            {
                GameObject obj = bfold.gameObject;
                BoxCollider[] colliders = obj.GetComponentsInChildren<BoxCollider>();
                foreach(BoxCollider c in colliders)
                {
                    GameObject blockSquare = Instantiate(SquareCollider, 
                                                c.transform.position, 
                                                go.transform.rotation);
                    blockSquare.GetComponent<SquareCast>().size = bfold.size;
                    blockSquare.name = "bs";
                    blockSquare.GetComponent<SquareCast>().showRay = true;
                    blockSquare.GetComponent<SquareCast>().customMask = bfold.customMask;
                    blockSquare.transform.position = blockSquare.transform.position + blockSquare.transform.rotation * c.center;
                    blockSquare.transform.parent = parent2.transform;
                    if(bfold.GetComponentInParent<CrystalShard>())
                        blockSquare.tag = "NoBlockPlayer";
                    copiesList.Add(blockSquare);
                    obstList.Add(blockSquare);
                    colliderDict.Add(blockSquare, c.size / 2);
                }
            }    
            //}
        }
        
        //C: checks for squares running into other stuff
        //Ideally we should check every point along the rotation axis, but this is not feasible. 
        float degrees = fd.degrees;

        for(int i = 1; i <= numChecks; i++) 
        {
            parent2.transform.RotateAround(fd.center, fd.axis, degrees/(numChecks+1));
            int j = 0;
            foreach(GameObject go in copiesList)
            {
                SquareCast sc = go.GetComponent<SquareCast>();
                RaycastHit hit;
                bool collide = sc.SquareRaycast(out hit, squareCollidingMask);
                if(collide) //C: We need to make sure the collision is with an object that is not part of the foldable objects group (these will move with the square)
                {
                    PaperSquare ps =  hit.transform.gameObject.GetComponentInParent<PaperSquare>();
                    //C: There are 2 cases:
                    //1: we hit the player. Then ps is null, and there is a collision
                    //2: we hit an object/paper square. Then we need to check to see if it is in the fold side objects
                    // if so, this collision doesn't matter. if not, then we can't fold
                    if(ps == null || !fd.foldObjects.foldSquares.Contains(ps.gameObject)) 
                    {
                        Debug.Log($"Collision with {hit.transform.gameObject.name} on ray {i},{j}.");
                        Destroy(parent2);
                        return false;
                    }
                    if(ps == null)
                        Debug.Log($"Collision with {hit.transform.gameObject.name} Ignored due to null paper square.");
                    else
                        Debug.Log($"Collision with {hit.transform.gameObject.name} Ignored due to same side collision.");
                }
                j++;               
            }
            foreach (GameObject go in obstList)
            {
                Collider[] hits = Physics.OverlapBox(go.transform.position, colliderDict[go], go.transform.rotation, squareCollidingMask);
                foreach(Collider c in hits)
                {
                    PaperSquare ps =  c.transform.gameObject.GetComponentInParent<PaperSquare>();
                    //if(invertFold && ps == null)
                      //  Debug.Log($"Collision with {hit.transform.gameObject.name} Ignored due to special case 1A"); //1a, ignore. 
                    if(ps == null && go.tag == "NoBlockPlayer");
                    else if(ps == null || !fd.foldObjects.foldSquares.Contains(ps.gameObject)) 
                    {
                        Debug.Log($"Collision with {c.transform.gameObject.name}.");
                        Destroy(parent2);
                        return false;
                    }
                }
            }
        }
        Destroy(parent2);
        Debug.Log("end collision check, no collisions found");
        return true;
    }
}

public class SidesInterlockedException : UnityException { }

public enum FoldFailureType {
    NONE, //Valid fold
    KINKED, //Could not physically fold along line
    PAPERCLIP, //folding paper through itself
    COLLISION, //hit object, player, paper, etc on fold path
    NOCHECK, //could not check fold due to another action occurring 
}
