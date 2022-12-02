using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowstickBox : MonoBehaviour
{
    public bool glowstickActive = false;

   /* private void OnTriggerEnter(Collider other) {
        if(glowstickActive && other.GetComponentInChildren<CrystalShard>()){
            print("Crystal in range");
            other.GetComponent<CrystalShard>().ActivateCrystal(true);
        }
    }

     

    private void OnTriggerExit(Collider other) {
        if(glowstickActive && other.GetComponentInChildren<CrystalShard>())
            other.GetComponent<CrystalShard>().ActivateCrystal(false);
    }*/

    private void OnTriggerStay(Collider other) {
        if(other.GetComponentInChildren<CrystalShard>()){
            other.GetComponent<CrystalShard>().ActivateCrystal(glowstickActive);
        }
    }
}
