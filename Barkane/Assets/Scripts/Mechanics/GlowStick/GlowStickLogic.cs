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

    private int crackFoldNum = -1;
    private int expirationFoldNum = -1;

    public class GlowStickArgs
    {
        public int lifetime;
        public GlowstickState state;
        public ActionCallEnum source;

        public GlowStickArgs(int l, GlowstickState s, ActionCallEnum souce)
        {
            lifetime = l;
            state = s;
            this.source = souce;
        }
    }

    public static event System.EventHandler<GlowStickArgs> OnGlowstickChange;

    private void OnEnable() {
        PaperStateManager.OnFold += OnFold;
    }

    private void OnDisable() {
        PaperStateManager.OnFold -= OnFold;
    }

    private void Start() {
        shards = FindObjectsOfType<CrystalShard>();
        paperJoint = GetComponentInParent<PaperJoint>();
    }

    //called whenever the joint the glowstick is on is folded;
    public void OnFold(object sender, PaperStateManager.FoldArgs args)
    {
        bool jointInFold = args.fd.axisJoints.Contains(paperJoint);
        if(args.source == ActionCallEnum.UNDO)
        {
            HandleUndoFold(jointInFold, args.foldnum, args.source);
        }
        else
        {
            HandleFold(jointInFold, args.foldnum, args.source);
        }

    
    }

    private void HandleUndoFold(bool jointInFold, int foldnum, ActionCallEnum source)
    {
        if(foldnum >= expirationFoldNum) return;
        else if(foldnum == expirationFoldNum - 1)
        {
            //return to cracked state
            if(state != GlowstickState.OFF)
                print("expiration fold not in off state. Bad.");
            state = GlowstickState.CRACKED;
            ToggleGSBoxes(true);
            GetComponent<GlowStick>().innerRenderer.material = materials[1];
            OnGlowstickChange?.Invoke(this, new GlowStickArgs(lifetime, state, source));
            lifetime++;
        }
        else if (foldnum >= crackFoldNum)
        {
            print("mid undo");
            //add lifetime
            // ToggleGSBoxes(true);
            //     GetComponent<GlowStick>().innerRenderer.material = materials[1];
            OnGlowstickChange?.Invoke(this, new GlowStickArgs(lifetime, state, source));
            lifetime++;
        }
        else if (foldnum == crackFoldNum - 1)
        {
            //reset to primed state
            state = GlowstickState.PRIMED;
            //lifetime++;
            GetComponent<GlowStick>().innerRenderer.material = materials[0];
            OnGlowstickChange?.Invoke(this, new GlowStickArgs(lifetime, state, source));
            ToggleGSBoxes(false);
            crackFoldNum = -1;
            expirationFoldNum = -1;
        }
        else return;
    }

    private void HandleFold(bool jointInFold, int foldnum, ActionCallEnum source)
    {
        if(state == GlowstickState.CRACKED)
        {
            lifetime--;
            OnGlowstickChange.Invoke(this, new GlowStickArgs(lifetime, state, source));
            Debug.Log($"Glowstick has {lifetime} folds left");
            if(lifetime == 0)
            {
                state = GlowstickState.OFF;
                OnGlowstickChange?.Invoke(this, new GlowStickArgs(lifetime, state, source));
                ToggleGSBoxes(false);
                GetComponent<GlowStick>().innerRenderer.material = materials[2];
            }
        }
        if(state == GlowstickState.PRIMED && jointInFold)
        {
            state = GlowstickState.CRACKED;
            ToggleGSBoxes(true);
                GetComponent<GlowStick>().innerRenderer.material = materials[1];
            OnGlowstickChange?.Invoke(this, new GlowStickArgs(lifetime, state, source));
            crackFoldNum = foldnum;
            expirationFoldNum = foldnum + lifetime;
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
