using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareCast : MonoBehaviour
{
    public int numRays = 10;
    public bool showRay = false;
    public LayerMask mask;
    public float size = 1.6f;

    public bool SquareRaycast(out RaycastHit hit, LayerMask squareCollidingMask, bool overrideMask = true)
    {
        RaycastHit h;
        for(int i = 0; i< numRays; i++)
        {
            Vector3 pos1 = this.transform.position - this.transform.forward * size/2 + this.transform.right * size * ((float)i /numRays - 0.5f + 0.5f/numRays);
            bool collide = Physics.Raycast(pos1, this.transform.forward, out h, size, overrideMask ? squareCollidingMask : mask);
            if(showRay && i % 20 == 0) Debug.DrawRay(pos1, this.transform.forward * size, Color.green, 20);
            if(collide)
            {
                Debug.DrawRay(pos1, this.transform.forward * size, Color.red, 30);
                Debug.Log($"Cannot Fold: hit {h.transform.gameObject.name} when calculating fold path");
                hit = h;
                return true;
            }
        }
        hit = new RaycastHit();
        return false;
    }
}
