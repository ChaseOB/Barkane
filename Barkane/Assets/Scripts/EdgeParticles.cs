using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the PaperSquares
// Finds the Edge Particle Prefab
// then iterates through the prefab's 8 children ParticleSystems
// makes them play or pause and clear

public class EdgeParticles : MonoBehaviour, BarkaneEditor.IRefreshable
{
    [SerializeField, HideInInspector] List<ParticleSystem> listOfSystems;
    [SerializeField, HideInInspector] GameObject edgeParticlesPrefabChild;
    bool isAwake = false;
    bool atCapacity = false;

    private void FindAllChildrenPS() {
        if (listOfSystems == null) Refresh();
    }

    public void Emit() {
        if (!isAwake) {
            isAwake = true;
            if (!atCapacity) {
                atCapacity = true;
                FindAllChildrenPS();
            }
            foreach (ParticleSystem ps in listOfSystems) {
                ps.Play();
            }
        }
    }

    public void Unemit() {
        foreach (ParticleSystem ps in listOfSystems) {
            ps.Pause();
            ps.Clear();
            ps.Play();
        }
        isAwake = false;
    }

    public void Refresh()
    {
        edgeParticlesPrefabChild = gameObject.transform.Find("Edge Particles").gameObject;
        listOfSystems = new List<ParticleSystem>();

        ParticleSystem[] sys = GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem ps in sys) {
            listOfSystems.Add(ps);
        }
    }
}
