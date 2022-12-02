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

    public LayerMask squareCollidingMask;

    public bool checkRaycast = false; //C: set to true when the rest of the check is good
    public bool raycastCheckDone = false;
    public FoldData foldData = new FoldData();
    public Coroutine checkCoroutine = null;
    public bool raycastCheckReturn = false;
    public bool crDone = false;



    private void Start() 
    {
        foldablePaper = FindObjectOfType<FoldablePaper>();
    }

    //C: Tries to fold the given objects. Returns true and folds if successful, returns false if this fold is not possible.
    public bool TryFold(FoldData fd)
    {
        Debug.Log("trying to fold");

        if(!ActionLockManager.Instance.TryTakeLock(this)) {
            Debug.Log($"Can't fold: lock taken by {ActionLockManager.Instance.LockObject}");
            return false;
        }
        //C: we need to wait until FixedUpdate to check the colliders. So we Call CCF, then if that passes, we know we've created collider data
        // that we need to call CheckColliders. If that passes, then it will call fold. 
        if(CheckCanFold(fd)) 
        {
            checkCoroutine = StartCoroutine(WaitForColliderCheck(fd));
            //AudioManager.Instance?.Play("Fold");
           // Fold(foldJoint, foldObjects, center, axis, degrees);
            return true;
        }
        else
        {
            AudioManager.Instance?.Play("Fold Error");
            //play error sound
        }
        ActionLockManager.Instance.TryRemoveLock(this);
        return false;
        
    }

    private void Update() {
        if(crDone && checkCoroutine != null)
        {
            crDone = false;
            StopCoroutine(checkCoroutine);
            checkCoroutine = null;
        }
    }

    public IEnumerator WaitForColliderCheck(FoldData fd)
    {
        Debug.Log("enter CR");
        yield return new WaitUntil(() => raycastCheckDone);
        Debug.Log("raycast done");
        raycastCheckDone = false;
        if(raycastCheckReturn){
            Fold(fd);
            raycastCheckReturn = false;
        }
        else
        {
            ActionLockManager.Instance.TryRemoveLock(this);
            AudioManager.Instance?.Play("Fold Error");
        }
        foldData = new FoldData();
        crDone = true;
    }

    public bool CheckCanFold(FoldData fd)
    {
        if(isFolding) {
            Debug.Log("Cannot fold: You can't do 2 folds at once");
            return false;
        }
        if(checkCoroutine != null){
            Debug.Log("Cannot fold: currently checking colliders");
            return false;
        }

        /* FOLD CHECK 1: KINKED JOINTS
            This checks that the selected joint lies within a single plane. If it does, this might be a valid fold. If not, 
            This is not a valid fold and we return false

            KNOWN ISSUES: None
        */
        HashSet<int> x = new HashSet<int>();
        HashSet<int> y = new HashSet<int>();
        HashSet<int> z = new HashSet<int>();

        foreach(PaperJoint pj in foldablePaper.PaperJoints)
        {
            if(pj.showLine)
            {
                x.Add(Vector3Int.RoundToInt(pj.transform.position).x);
                y.Add(Vector3Int.RoundToInt(pj.transform.position).y);
                z.Add(Vector3Int.RoundToInt(pj.transform.position).z);
            }
        }

        if((x.Count > 1 && y.Count > 1) || (x.Count > 1 && z.Count > 1) || (z.Count > 1 && y.Count > 1)) {
            Debug.Log($"Cannot fold: joint is kinked. {x.Count} {y.Count} {z.Count}");
            return false;
        }

        //handles clipping of back-to-back paper
        if(!CheckPaperClipping(fd))
            return false;

        //C: need to transfer data out to be used for raycast stuff
        foldData = fd;
        checkRaycast = true;
        return true;
    }

    /* FOLD CHECK 2: PAPER CLIPPING
            This checks that back to back squares do not clip through eachother. This is because the squares are so
            thin that squares at the same location will not be caught using the raycast, thus another approach is needed.

            This part of the check also updates the visibile faces of each square
            KNOWN ISSUES: 
            -pretty good with 2 squares, not so much for higher quantities 
            -sorting is straight up wrong sometimes. 
        */
    private bool CheckPaperClipping(FoldData fd)
    {
        List<List<PaperSquare>> overlaps = foldablePaper.FindOverlappingSquares();
        foreach(List<PaperSquare> list in overlaps)
        {
            //Vector3 intial = Vector3.zero;
            //Vector3 newVec = Vector3.zero;
            if(list.Count > 1) //C: if count = 1 then only 1 square, can't fold through itself
            {
                Debug.Log("Check Clip Square");
                GameObject parent = new GameObject();
                parent.transform.position = fd.center;
                List<GameObject> activeSides = new List<GameObject>();
                GameObject t1 = new GameObject();
                GameObject t2 = new GameObject();
                foreach(PaperSquare ps in list) 
                {
                    if(ps.BottomHalf.activeSelf)
                        activeSides.Add(ps.BottomHalf);
                    if(ps.TopHalf.activeSelf)
                        activeSides.Add(ps.TopHalf);
                }
               //if(activeSides.Count != 2){
               //     Debug.LogError($"!2 active sides in a single location (this should not happen). Count: {activeSides.Count}");
               // }
                //C: if the active sides of this stack are both in or both out of the fold, then they won't clip
                if(activeSides.Count == 2 &&
                    fd.foldObjects.foldSquares.Contains(activeSides[0].GetComponentInParent<PaperSquare>().gameObject)
                    != fd.foldObjects.foldSquares.Contains(activeSides[1].GetComponentInParent<PaperSquare>().gameObject))
                {
                    /*C: Else, check position of the ends of the normal vectors before and after fold
                    // if there is no clipping, then the points at the ends of the normals will be farther apart (point away)
                    // than if there was clipping (point towards eachother). So we can check this fold and the other fold direction
                    // by folding 180* after the intial fold and then comapare distances
                    */
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

                    print($"{d1}, {d2}");
               
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
            }
        }
        return true;
    }

    private void FixedUpdate() {
        if(checkRaycast)
            raycastCheckReturn = CheckRayCast();
    }

    private bool CheckRayCast() {
        return CheckRayCast(foldData.foldObjects); 
        //&& CheckRayCast(foldData.playerFoldObjects, true);
    }

    /* FOLD CHECK 3: PAPER AND OBJECT COLLISION
            This checks that Paper squares do not clip through other squares or obstacles. This also checks that obstacles do not clip 
            through squares. Becuas it is easier to check for squares colliding with objects than vice versa, 
            this check is run twice, once with the "real" fold and once by folding the player side object in 
            the opposite direction 

            KNOWN ISSUES: 
            -won't let you fold crystal into player
            -clipping squares through objects is hard, but clipping objects into squares is easy (fixed?)
        */
    private bool CheckRayCast(FoldObjects fo, bool invertFold = false) {
        Debug.Log("checking raycast...");
        checkRaycast = false;
        int numChecks = 10;
        
        GameObject parent2 = new GameObject("parent 2");
        parent2.transform.position = foldData.center;
        List<GameObject> copiesList = new List<GameObject>();
        List<GameObject> obstList = new List<GameObject>();
        Dictionary<GameObject, Vector3> colliderDict = new Dictionary<GameObject, Vector3>();
        foreach(GameObject go in fo.foldSquares)
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
                    Debug.Log(c.center);
                    blockSquare.name = "bs";
                    blockSquare.GetComponent<SquareCast>().showRay = true;
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
        float degrees = foldData.degrees;
        if(invertFold)
            degrees *= -1;
        for(int i = 1; i <= numChecks; i++) 
        {
            parent2.transform.RotateAround(foldData.center, foldData.axis, degrees/(numChecks+1));
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
                    //1a: if we are on the player side fold, ignore this case
                    //2: we hit an object/paper square. Then we need to check to see if it is in the fold side objects
                    // if so, this collision doesn't matter. if not, then we can't fold
                    //print(invertFold);
                    if(invertFold && ps == null)
                        Debug.Log($"Collision with {hit.transform.gameObject.name} Ignored due to special case 1A"); //1a, ignore. 
                    else if(ps == null || !fo.foldSquares.Contains(ps.gameObject)) 
                    {
                        Debug.Log($"Collision with {hit.transform.gameObject.name} on ray {i},{j}. Invert  {invertFold}");
                        Destroy(parent2);
                        raycastCheckDone = true;
                        return false;
                    }
                    if(ps == null)
                        Debug.Log($"Collision with {hit.transform.gameObject.name} Ignored due to null paper square. Invert  {invertFold}");
                    else
                        Debug.Log($"Collision with {hit.transform.gameObject.name} Ignored due to same side collision. Invert  {invertFold}");
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
                    else if(ps == null || !fo.foldSquares.Contains(ps.gameObject)) 
                    {
                        Debug.Log($"Collision with {c.transform.gameObject.name}. Invert  {invertFold}");
                        Destroy(parent2);
                        raycastCheckDone = true;
                        return false;
                    }
                }
            }
        }
        Destroy(parent2);
        Debug.Log("end collision check, no collisions found");

        raycastCheckDone = true;
        return true;
    }

    public void Fold(FoldData fd, bool fromStack = false, bool undo = false)
    {
        if(!isFolding) 
        {
            var foldJointRenderer = fd.foldJoint.JointRenderer;
            if(foldJointRenderer != null)
                StartCoroutine(FoldHelper(fd, fromStack, undo, fd.foldObjects.DisableJointMeshes, fd.foldObjects.EnableJointMeshes));
            else
                StartCoroutine(FoldHelper(fd, fromStack, undo));
        }
            
    }

    
    private IEnumerator FoldHelper(FoldData fd, bool fromStack = false, bool undo = false, System.Action beforeFold = null, System.Action afterFold = null)
    {
        FoldObjects objectsToFold = fd.foldObjects;
        Vector3 center = fd.center;

        AudioManager.Instance?.Play("Fold");
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

        StoreAllSquarePos();
        float t = 0;
        int wait = 1;
        fd.foldJoint.OnFold();
        while (t < foldDuration)
        {
            t += Time.deltaTime;
            tempObj.transform.RotateAround(center, fd.axis, (fd.degrees / foldDuration) * Time.deltaTime);
            wait--;
            if(wait == 0){
                UpdateSquareVisibility(objectsToFold);
            }
            yield return null;
        }
        

        target.transform.RotateAround(center, fd.axis, fd.degrees);
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
        if(undo)
            foldCount--;
        else
            foldCount++;
        UIManager.UpdateFoldCount(foldCount);
        if(!fromStack && !undo) {
            UndoRedoManager.Instance?.AddAction(fd);
        }
        ActionLockManager.Instance.TryRemoveLock(this);
    }

    private void StoreAllSquarePos()
    {
        foreach(PaperSquare ps in foldablePaper.PaperSquares)
        {
            ps.StorePosition(ps.transform.position);
        }
    }

    private void UpdateSquareVisibility(FoldObjects foldObjects)
    {
        List<List<PaperSquare>> overlaps = foldablePaper.FindOverlappingSquares();

        foreach(PaperSquare ps in foldablePaper.PaperSquares)
            ps.ForceRefsUpdate();
    
        foreach(List<PaperSquare> list in overlaps)
        {
            if(list.Count == 1) //C: only 1 square, enable both meshes
            {
                list[0].topStack = null;
                list[0].bottomStack = null;
                list[0].ToggleBottom(true);
                list[0].ToggleTop(true);
            }
            
            else
            {
                Debug.Log($"{list.Count} squares at location {list[0].transform.position}");
                    //We arbitrarily pick one side of the first square to be the "top", which is then used as a comparison for other square's top/bottoms
                Vector3 topHalfNorm = list[0].TopHalf.transform.up;
                    
                HashSet<GameObject> activeSides = new HashSet<GameObject>();
                HashSet<GameObject> activeFoldSides = new HashSet<GameObject>();

                GameObject foldTop = null;
                GameObject foldBot = null;
                GameObject stationaryTop = null;
                GameObject stationaryBot = null;

                Vector3 prevPos = Vector3.zero;
                //C: there should be a total of 2 sides enabled in each list currently. We need to figure out which of them to now hide.
                foreach(PaperSquare ps in list) 
                {
                    activeSides.UnionWith(ps.GetOpenSides(!CoordUtils.ApproxSameVector(topHalfNorm, ps.TopHalf.transform.up)));
                    if(foldObjects.foldSquares.Contains(ps.gameObject))
                    {
                         //   Debug.Log($"{ps.gameObject.name} is in fold list on stack of size {list.Count}");
                        activeFoldSides.UnionWith(ps.GetOpenSides(!CoordUtils.ApproxSameVector(topHalfNorm, ps.TopHalf.transform.up)));
                        prevPos = ps.storedPos;
                    }
                }
                    
                Debug.Log("fold side " + activeFoldSides.Count);
                Debug.Log("total " + activeSides.Count);
                foreach(GameObject go in activeSides)
                {
                    if(activeFoldSides.Contains(go))
                    {
                            //Debug.Log(go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name + " is in active fold sides");
                        if(CoordUtils.ApproxSameVector(topHalfNorm, go.transform.up))
                        {
                            foldTop = go;
                              //  Debug.Log("fold top is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
                        }
                        else
                        {
                            foldBot = go;
                             //   Debug.Log("fold bot is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
                        }
                    }
                    else
                    {
                        if(CoordUtils.ApproxSameVector(topHalfNorm, go.transform.up))
                        {
                            stationaryTop = go;
                               // Debug.Log("stat top is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
                        }
                        else
                        {
                            stationaryBot = go;
                               // Debug.Log("stat bot  is " + go.GetComponentInParent<PaperSquare>().gameObject.name + " " + go.name);
                        }
                    }
                }
                if(foldTop == null)
                    Debug.Log("fold top is null");
                if(foldBot == null)
                    Debug.Log("fold bot is null");
                if(stationaryTop == null)
                    Debug.Log("stat top is null");
                if(stationaryBot == null)
                    Debug.Log("stat bot is null");

                if(foldTop != null && foldBot != null && stationaryTop != null && stationaryBot != null)
                {
                    float topDist = Vector3.Magnitude(prevPos - stationaryTop.transform.position);
                    float botDist = Vector3.Magnitude(prevPos - stationaryBot.transform.position);
                    if(topDist == botDist) 
                        Debug.Log("same dist");
                    //foldTop.GetComponentInParent<PaperSquare>().invertForGetOpen = !CoordUtils.ApproxSameVector(foldTop.transform.up, foldTop.GetComponentInParent<PaperSquare>().TopHalf.transform.up);
                    if(topDist > botDist) //new square is on the bottom, join top of fold and bottom of stationary
                    {
                        Debug.Log("top side");
                        foldTop.SetActive(false);
                        stationaryBot.SetActive(false);
                        PaperSquare foldSquare = foldTop.GetComponentInParent<PaperSquare>();
                        PaperSquare statSquare = stationaryBot.GetComponentInParent<PaperSquare>();
                        foldSquare.bottomStack = statSquare;
                        statSquare.topStack = foldSquare;
                        foldSquare.UpdateHitboxes();
                        statSquare.UpdateHitboxes();
                    }
                    else //new fold square is on the top, join bot of fold and top of stationary
                    {
                        Debug.Log("bottom side");
                        foldBot.SetActive(false);
                        stationaryTop.SetActive(false);
                        PaperSquare foldSquare = foldBot.GetComponentInParent<PaperSquare>();
                        PaperSquare statSquare = stationaryTop.GetComponentInParent<PaperSquare>();
                        foldSquare.topStack = statSquare;
                        statSquare.bottomStack = foldSquare;
                        foldSquare.UpdateHitboxes();
                        statSquare.UpdateHitboxes();
                    }
                }
            }
        }
    }

   
}

//C: we should pass this insead of a bunch of params but i have 90 min to make this game work aaaaa
public class FoldData: Action 
{
    public PaperJoint foldJoint;
    public FoldObjects foldObjects;
    public FoldObjects playerFoldObjects;
    public Vector3 center;
    public Vector3 axis;
    public float degrees;

    public FoldData() {}

    public FoldData(PaperJoint fj, FoldObjects fo, FoldObjects pfo, Vector3 c, Vector3 a, float deg) {
        foldJoint = fj;
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



