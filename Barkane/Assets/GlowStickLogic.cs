using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowStickLogic : MonoBehaviour
{
    public int lifetime = 4; //the number of folds this glowstick will be active for
    public GlowstickState state = GlowstickState.PRIMED;
    public BoxCollider box1;
    public BoxCollider box2;
    public MeshRenderer innerRenderer;
    public List<Material> materials = new List<Material>();
    //called whenever the joint the glowstick is on is folded;
    public void OnFold(PaperJoint callJoint, PaperJoint foldJoint)
    {
        if(state == GlowstickState.CRACKED)
        {
            lifetime--;
            Debug.Log($"Glowstick has {lifetime} folds left");
            if(lifetime == 0)
            {
                state = GlowstickState.OFF;
                ToggleGSBoxes(false);
                innerRenderer.material = materials[2];
                foreach (CrystalShard shard in FindObjectsOfType<CrystalShard>())
                    shard.ActivateParticles(false);
            }
        }
        if(state == GlowstickState.PRIMED && callJoint == foldJoint)
        {
            state = GlowstickState.CRACKED;
            ToggleGSBoxes(true);
            innerRenderer.material = materials[1];
            foreach (CrystalShard shard in FindObjectsOfType<CrystalShard>())
                shard.ActivateParticles(true);
        }
    }

    //toggles the boxes which activate the crystals;
    public void ToggleGSBoxes(bool toggle)
    {
       // box1.enabled = toggle;
       // box2.enabled = toggle;
    }
}

public enum GlowstickState {
    PRIMED,
    CRACKED,
    OFF,
}
