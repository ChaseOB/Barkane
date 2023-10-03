using BarkaneEditor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Transparent outline used to show the position of a fold
public class FoldIndicator : MonoBehaviour
{
    [SerializeField] private GameObject ghostSquarePrefab;
    private List<Vector3Int> locs = new List<Vector3Int>();
    public Vector3 Center = Vector3.zero;
    public Material goodMat;
    public Material badMat;
    public List<MeshRenderer> meshRenderers;

    public void ToggleIndicatorState(bool active)
    {
        foreach(MeshRenderer m in meshRenderers)
        {
            m.material = active ? goodMat : badMat;
        }
    }


    public void BuildIndicator(List<FoldPositionData> fd)
    {
        foreach(FoldPositionData data in fd)
        {
            if(data.obj is SquareData)
            {
                SquareData s = (SquareData) data.obj;
            
            if(s.currentPosition.location != data.target.location && !locs.Contains(data.target.location))
            {
                // print("diff location" + s.targetPosition.location);
                GameObject newSquare = Instantiate(ghostSquarePrefab, data.target.location, data.target.rotation);
                meshRenderers.Add(newSquare.GetComponent<MeshRenderer>());
                var ghostSquareRenderer = newSquare.GetComponent<MeshRenderer>();
                ghostSquareRenderer.sharedMaterial = VFXManager.Theme.GhostMat;
                newSquare.transform.parent = gameObject.transform;
                locs.Add(data.target.location);
            }
            }
            // else{
            //     print(data.obj);
            // }
        }
        Center = CoordUtils.CalculateCenter(locs);
    }
}
