using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowstickBox : MonoBehaviour
{
    public bool glowstickActive = false;
    private bool glowstickActiveLastFrame = false;

    [SerializeField] private List<ParticleSystem> particleSystems;

    private void OnTriggerStay(Collider other) {
        if(other.GetComponentInChildren<CrystalShard>()){
            other.GetComponent<CrystalShard>().ActivateCrystal(glowstickActive);
        }
    }

    private void Update() {
        if(glowstickActiveLastFrame != glowstickActive){
            if(glowstickActive)
                EnableBox();
            else
                DisableBox();
        }
        glowstickActiveLastFrame = glowstickActive;
    }

    private void EnableBox()
    {
        foreach(ParticleSystem ps in particleSystems)
            ps.Play();
    }

    private void DisableBox()
    {
        foreach(ParticleSystem ps in particleSystems)
            ps.Stop();
    }

}
