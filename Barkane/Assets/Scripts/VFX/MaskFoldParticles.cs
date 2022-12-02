using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskFoldParticles : MonoBehaviour, BarkaneEditor.IRefreshable
{
    [SerializeField] List<ParticleSystem> listOfSystems;
    private bool isAwake;
    private Theme theme;

    public void UpdateTheme(Theme t)
    {
        if(t != theme) {
            theme = t;
        }

        foreach (ParticleSystem ps in listOfSystems)
        {
            ps.GetComponent<ParticleSystemRenderer>().material = theme.JointParticle;
        }

    }
    public void Emit()
    {
        if (!isAwake)
        {
            isAwake = true;
            if (listOfSystems == null) Refresh();
        }
        foreach (ParticleSystem ps in listOfSystems)
        {
            // ps.GetComponent<ParticleSystemRenderer>().material = theme.JointParticle;
            ps.Play();
        }
    }

    public void UnEmit()
    {
        foreach (ParticleSystem ps in listOfSystems)
        {
            ps.Pause();
            ps.Clear();
        }
    }

    public void Refresh()
    {
        listOfSystems = new List<ParticleSystem>();
        
        ParticleSystem[] sys = GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem ps in sys) {
            listOfSystems.Add(ps);
        }
    }
}
