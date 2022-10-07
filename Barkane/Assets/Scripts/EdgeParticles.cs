using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the PaperSquares
// Finds the Edge Particle Prefab
// then iterates through the prefab's 8 children ParticleSystems
// makes them play or pause and clear

public class EdgeParticles : MonoBehaviour, BarkaneEditor.IRefreshable
{
    private int count = 8;
    [SerializeField, HideInInspector] List<ParticleSystem> listOfSystems;
    [SerializeField, HideInInspector] GameObject edgeParticlesPrefabChild;
    bool isAwake = false;
    bool atCapacity = false;

    private void FindAllChildrenPS() {
        if (listOfSystems == null || listOfSystems.Count != count) Refresh();
    }

    public void Emit() {
        if (!isAwake) {
            isAwake = true;
            if (!atCapacity) {
                atCapacity = true;
                FindAllChildrenPS();
                count = listOfSystems.Count;
            }
            for (int i = 0; i < count; i++) {
                listOfSystems[i].Play();
            }
        }
    }

    public void Unemit() {
        for (int i = 0; i < count; i++) {
            isAwake = false;
            listOfSystems[i].Pause();
            listOfSystems[i].Clear();
        }
    }

    public void Refresh()
    {
        edgeParticlesPrefabChild = gameObject.transform.Find("Edge Particles").gameObject;
        Transform result;
        listOfSystems = new List<ParticleSystem>();

        for (int i = 1; i < (count + 1); i++)
        {
            string ps;

            ps = "Particle System " + i.ToString();
            result = edgeParticlesPrefabChild.transform.Find(ps);

            if (result)
            {
                listOfSystems.Add(result.gameObject.GetComponent<ParticleSystem>());
            } else
            {
                throw new UnityException($"Cannot find particle system {i} prefab under paper square");
            }
        }
    }
}
