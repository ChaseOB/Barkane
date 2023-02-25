using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalShard : MonoBehaviour, IThemedItem
{
    [SerializeField] public Goal goal; 
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject inactiveModel;
    [SerializeField] private List<Material> materials;

    public bool crystalActive = true; //C: True except when in caves and no glowstick in area
    public bool particlesActive = true; //C: True except when in caves and no glowstick active

    public float displacement = 0.1f;
    public float hoverSpeed = 1.0f;
    public float rotateSpeed = 1.0f;
    private Vector3 prevVal = Vector3.zero;

    [SerializeField] private new ParticleSystem particleSystem;
    public List<Gradient> themePartColors = new List<Gradient>();

    private void OnEnable() {
        GlowStickLogic.OnGlowstickChange += OnGlowstickChange;
    }

    private void OnDisable() {
        GlowStickLogic.OnGlowstickChange -= OnGlowstickChange;
    }

    private void Start() {
        goal = FindObjectOfType<Goal>();
        ActivateParticles(particlesActive);
        ActivateCrystal(crystalActive);
    }
    
    private void Update() {
        AnimatePosition();
    }

    private void AnimatePosition()
    {
        Vector3 currentVal = new Vector3(0, displacement * Mathf.Sin(Mathf.PI * hoverSpeed * Time.time));
        model.transform.localPosition += currentVal - prevVal;
        inactiveModel.transform.localPosition += currentVal - prevVal;
        prevVal = currentVal;
        model.transform.Rotate(Vector3.up, rotateSpeed * 0.1f);
        inactiveModel.transform.Rotate(Vector3.up, rotateSpeed * 0.1f);
    }
    
    public void UpdateTheme(Theme t) {
        model.GetComponentInChildren<MeshRenderer>().material = t.crystalMat;
        var col = particleSystem.colorOverLifetime;
        col.color = themePartColors[(int)t.themeEnum];
    }

    public void Collect()
    {
        if(crystalActive)
        {
            goal?.CollectShard();
            this.gameObject.SetActive(false);
            AudioManager.Instance?.Play("Ding");
        }
    }

    public void ActivateParticles(bool val)
    {
        particlesActive = val;
        if(particlesActive){
            particleSystem.Play();
        }
        else {
            particleSystem.Stop();
        }
    }

    public void ActivateCrystal(bool val)
    {
        crystalActive = val;
        if(crystalActive) {
            model.SetActive(true);
            inactiveModel.SetActive(false);
        }
        else
        {
            model.SetActive(false);
            inactiveModel.SetActive(true);
        }
    }

    private void OnGlowstickChange(object sender, GlowStickLogic.GlowStickArgs e) {
        if(e.state == GlowstickState.OFF)
            ActivateParticles(false);
        if(e.state == GlowstickState.CRACKED)
            ActivateParticles(true);
    }
}
