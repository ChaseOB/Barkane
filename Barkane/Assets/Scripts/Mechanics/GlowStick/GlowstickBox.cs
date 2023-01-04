using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowstickBox : MonoBehaviour
{
    public bool glowstickActive = false;

    private void OnTriggerStay(Collider other) {
        if(other.GetComponentInChildren<CrystalShard>()){
            other.GetComponent<CrystalShard>().ActivateCrystal(glowstickActive);
        }
    }
}
