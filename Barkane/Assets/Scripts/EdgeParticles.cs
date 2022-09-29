using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Lives on the PaperSquares
// Finds the Edge Particle Prefab
// then iterates through the prefab's 4 children ParticleSystems
// makes them play or pause and clear

public class EdgeParticles : MonoBehaviour
{
    private int count = 4;
    List<ParticleSystem> listOfSystems = new List<ParticleSystem>();
    GameObject edgeParticlesPrefabChild;
    bool isAwake = false;
    bool atCapacity = false;

    void Start()
    {
        edgeParticlesPrefabChild = gameObject.transform.Find("Edge Particles").gameObject;
    }
    
    void Update()
    {

    }


    private void FindAllChildrenPS() {
        Transform result;

        for (int i = 1; i < 5; i++)
        {
            string ps;

            ps = "Particle System " + i.ToString();
            result = edgeParticlesPrefabChild.transform.Find(ps);

            if (result){
                listOfSystems.Add(result.gameObject.GetComponent<ParticleSystem>());
            }
        }
    }

    public void Emit() {
        if (!isAwake) {
            // print("in Emit, in !isAwake");
            isAwake = true;
            if (!atCapacity) {
                // print("in Emit, in !isAwake, in !isCapacity");
                atCapacity = true;
                FindAllChildrenPS();
                count = listOfSystems.Count;
                // print("listOfSystems.Count "  + count);
                string sysNames = "(emit) all the systems: ";
                for (int i = 0; i < count; i++) {
                    sysNames += listOfSystems[i].name + ", ";
                }
                // print(sysNames + "]");
            }
            for (int i = 0; i < count; i++) {
                // print("now making system " + listOfSystems[i].name + " play");
                listOfSystems[i].Play();
            } // does play have to be taken outside the !isAwake?
        }
        // print("outside of the !isAwake in emit " + gameObject.name);
    }

    public void Unemit() {
        for (int i = 0; i < count; i++) {
            // print("unemit current index is " + i.ToString() + ", with system " + listOfSystems[i].name);
            isAwake = false;
            listOfSystems[i].Pause();
            listOfSystems[i].Clear();
            // listOfSystems.Clear();
            // atCapacity = false;
        }
    }
}
