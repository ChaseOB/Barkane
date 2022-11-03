using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalShard : MonoBehaviour
{
    [SerializeField] public Goal goal; 
    [SerializeField] private GameObject model;
    [SerializeField] private GameObject inactiveModel;

    public bool crystalActive = true; //C: True except when in caves and no glowstick in area
    public bool particlesActive = true; //C: True except when in caves and no glowstick active

    public float displacement = 0.1f;
    public float hoverSpeed = 1.0f;
    public float rotateSpeed = 1.0f;
    private Vector3 prevVal = Vector3.zero;

    [SerializeField] private new ParticleSystem particleSystem;

    private void Start() {
        goal = FindObjectOfType<Goal>();
        ActivateParticles(particlesActive);
        ActivateCrystal(crystalActive);
    }
    
    private void Update() {
        Vector3 currentVal = new Vector3(0, displacement * Mathf.Sin(Mathf.PI * hoverSpeed * Time.time));
        model.transform.localPosition += currentVal - prevVal;
        inactiveModel.transform.localPosition += currentVal - prevVal;
        prevVal = currentVal;
        model.transform.Rotate(Vector3.up, rotateSpeed * 0.1f);
        inactiveModel.transform.Rotate(Vector3.up, rotateSpeed * 0.1f);
    }

    public void Collect()
    {
        if(crystalActive)
        {
            goal.CollectShard();
            Destroy(this.gameObject);
        }
    }

    public void ActivateParticles(bool val)
    {
        particlesActive = val;
        if(particlesActive){
            particleSystem.Play();
        }
        else {
            particleSystem.Pause();
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
}
