using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareCast : MonoBehaviour
{
    public int numRays = 10;
    public bool showRay = false;
    public LayerMask mask;
    public float size = 1.6f;
    public bool customMask = false;

    public bool SquareRaycast(out List<RaycastHit> hits, LayerMask squareCollidingMask)
    {
        hits = new();
        RaycastHit h;
        for(int i = 0; i< numRays; i++)
        {
            Vector3 pos1 = this.transform.position - this.transform.forward * size/2 + this.transform.right * size * ((float)i /numRays - 0.5f + 0.5f/numRays);
            bool collide = Physics.Raycast(pos1, this.transform.forward, out h, size, customMask ? mask : squareCollidingMask);
            if(showRay) Debug.DrawRay(pos1, this.transform.forward * size, Color.green, 20);
            if(collide)
            {
                Debug.DrawRay(pos1, this.transform.forward * size, Color.red, 30);
//                Debug.Log($"Cannot Fold: hit {h.transform.gameObject.name} when calculating fold path");
                hits.Add(h);
            }
        }
        for(int i = 0; i< numRays; i++)
        {
            Vector3 pos1 = this.transform.position - this.transform.right * size/2 + this.transform.forward * size * ((float)i /numRays - 0.5f + 0.5f/numRays);
            bool collide = Physics.Raycast(pos1, this.transform.right, out h, size, customMask ? mask : squareCollidingMask);
            if(showRay) Debug.DrawRay(pos1, this.transform.right * size, Color.green, 20);
            if(collide)
            {
                Debug.DrawRay(pos1, this.transform.right * size, Color.red, 30);
//                Debug.Log($"Cannot Fold: hit {h.transform.gameObject.name} when calculating fold path");
                hits.Add(h);
            }
        }
        return hits.Count > 0;
    }
}
