using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SquareCast : MonoBehaviour
{
    public int numRays = 10;
    private bool showRay = true;

    public bool SquareRaycast(out RaycastHit hit, LayerMask squareCollidingMask)
    {
        RaycastHit h;
        for(int i = 0; i< numRays; i++)
        {
            Vector3 pos1 = this.transform.position - this.transform.forward * 0.9f + this.transform.right * 1.8f * ((float)i /numRays - 0.5f + 0.5f/numRays);
            bool collide = Physics.Raycast(pos1, this.transform.forward, out h, 1.8f, squareCollidingMask);
            if(showRay && i == 4) Debug.DrawRay(pos1, this.transform.forward * 1.8f, Color.green, 10);
            if(collide)
            {
                Debug.DrawRay(pos1, this.transform.forward * 1.8f, Color.red, 10);
                Debug.Log($"Cannot Fold: hit {h.transform.gameObject.name} when calculating fold path");
                hit = h;
                return true;
            }
        }
        hit = new RaycastHit();
        return false;
    }
}
