using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlowStickLogic : MonoBehaviour
{
    public int lifetime = 4; //the number of folds this glowstick will be active for
    public GlowstickState state = GlowstickState.PRIMED;
    public GlowstickBox box1;
    public GlowstickBox box2;
    public List<Material> materials = new List<Material>();
    private CrystalShard[] shards;
    private PaperJoint paperJoint;

    public class GlowStickArgs
    {
        public int lifetime;
        public GlowstickState state;

        public GlowStickArgs(int l, GlowstickState s)
        {
            lifetime = l;
            state = s;
        }
    }

    public static event System.EventHandler<GlowStickArgs> OnGlowstickChange;

    private void OnEnable() {
        FoldAnimator.OnFold += OnFold;
    }

    private void OnDisable() {
        FoldAnimator.OnFold += OnFold;
    }

    private void Start() {
        shards = FindObjectsOfType<CrystalShard>();
        paperJoint = GetComponentInParent<PaperJoint>();
    }

    //called whenever the joint the glowstick is on is folded;
    public void OnFold(object sender, FoldAnimator.FoldArgs args)
    {
        if(state == GlowstickState.CRACKED)
        {
            lifetime--;
            Debug.Log($"Glowstick has {lifetime} folds left");
            if(lifetime == 0)
            {
                state = GlowstickState.OFF;
                ToggleGSBoxes(false);
                GetComponent<GlowStick>().innerRenderer.material = materials[2];
                OnGlowstickChange.Invoke(this, new GlowStickArgs(lifetime, state));
                //foreach (CrystalShard shard in shards)
                   // shard.ActivateParticles(false);
            }
        }
        if(state == GlowstickState.PRIMED && args.fd.axisJoints.Contains(paperJoint))
        {
            state = GlowstickState.CRACKED;
            ToggleGSBoxes(true);
                GetComponent<GlowStick>().innerRenderer.material = materials[1];
            OnGlowstickChange?.Invoke(this, new GlowStickArgs(lifetime, state));
            //foreach (CrystalShard shard in shards)
              //  shard.ActivateParticles(true);
        }
    }

    //toggles the boxes which activate the crystals;
    public void ToggleGSBoxes(bool toggle)
    {
        box1.glowstickActive = toggle;
        box2.glowstickActive = toggle;
    }
}

public enum GlowstickState {
    PRIMED,
    CRACKED,
    OFF,
}
