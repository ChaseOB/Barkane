
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class FoldData
{
    public List<PaperJoint> axisJoints; //all joints along axis
    public List<FoldableObject> foldObjects;
    public List<FoldableObject> playerFoldObjects;
    public Vector3Int axisPosition;
    public Vector3Int axisVector;
    public int degrees;
    


    public FoldData(List<PaperJoint> aj, List<FoldableObject> fo, List<FoldableObject> pfo, Vector3Int apos, Vector3Int avec, int deg) {
        axisJoints = aj;
        foldObjects = fo;
        playerFoldObjects = pfo; 
        axisPosition = apos;
        axisVector = avec;
        degrees = deg;
    }

}



// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class FoldData: Action 
// {
//     public List<PaperJoint> axisJoints; //all joints along axis
//     public FoldObjects foldObjects;
//     public FoldObjects playerFoldObjects;
//     public Vector3 center;
//     public Vector3 axis;
//     public float degrees;

//     public FoldData() {}

//     public FoldData(List<PaperJoint> aj, FoldObjects fo, FoldObjects pfo, Vector3 c, Vector3 a, float deg) {
//         axisJoints = aj;
//         foldObjects = fo;
//         playerFoldObjects = pfo; 
//         center = c;
//         axis = a;
//         degrees = deg;
//     }

//     public override Action GetInverse()
//     {
//         FoldData fd = (FoldData)this.MemberwiseClone();
//         fd.degrees *= -1;
//         return (Action)fd;
//     }

//     public override void ExecuteAction(bool undo)
//     {
//        GameObject.FindObjectOfType<FoldAnimator>().Fold(this, true, undo);
//     }

//     public Dictionary<Vector3Int, HashSet<PaperSquare>> FindOverlappingSquares()
//     {
//         List<PaperSquare> paperSquares = new List<PaperSquare>();
//         paperSquares.AddRange(foldObjects.squareScripts);
//         paperSquares.AddRange(playerFoldObjects.squareScripts);

//         Dictionary<Vector3Int, HashSet<PaperSquare>> posToSquares = new Dictionary<Vector3Int, HashSet<PaperSquare>>();

//         foreach(PaperSquare ps in paperSquares)
//         {
//             Vector3Int loc = Vector3Int.RoundToInt(ps.gameObject.transform.position);
//             if(!posToSquares.ContainsKey(loc))
//             {
//                 HashSet<PaperSquare> set = new HashSet<PaperSquare>();
//                 set.Add(ps);
//                 posToSquares.TryAdd(loc, set);
//             }
//             else
//             {
//                 posToSquares[loc].Add(ps);
//             }
//         }

//         return posToSquares;
//       /*  List<HashSet<PaperSquare>> overlapList = new List<HashSet<PaperSquare>>();

//         Dictionary<Vector3, HashSet<PaperSquare>> dict = new Dictionary<Vector3, HashSet<PaperSquare>>();

//         List<PaperSquare> paperSquares = new List<PaperSquare>();
//         foreach(GameObject go in foldObjects.foldSquares)
//             paperSquares.Add(go.GetComponent<PaperSquare>());
//         foreach(GameObject go in playerFoldObjects.foldSquares)
//             paperSquares.Add(go.GetComponent<PaperSquare>());
            
//         foreach(PaperSquare ps in paperSquares) {
//             bool didAdd = false;
//             foreach(Vector3 key in dict.Keys){
//                 if (Vector3.Magnitude(key - ps.transform.position) < 0.0001f) {
//                     dict[key].Add(ps);
//                     didAdd = true;
//                     Debug.Log("overlapping squares");
//                 }
//             }
//             if(!didAdd)
//             {
//                 HashSet<PaperSquare> set = new HashSet<PaperSquare>();
//                 set.Add(ps);
//                 dict.Add(ps.transform.position, set);
//             }
//         }

//         foreach (HashSet<PaperSquare> set in dict.Values){
//                 overlapList.Add(set);
//         }
//         return overlapList;*/
//     }
// }
