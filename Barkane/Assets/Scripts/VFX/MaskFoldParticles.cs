using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaskFoldParticles : MonoBehaviour, BarkaneEditor.IRefreshable
{
    [SerializeField] List<ParticleSystem> listOfSystems;
    private bool isAwake;
    
    public void Emit()
    {
        if (!isAwake)
        {
            isAwake = true;
            if (listOfSystems == null) Refresh();
        }
        foreach (ParticleSystem ps in listOfSystems)
        {
            ps.Play();
        }
    }

    public void UnEmit()
    {
        foreach (ParticleSystem ps in listOfSystems)
        {
            ps.Stop();
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

    public void EditorRefresh()
    {
        Refresh();
    }

    public void RuntimeRefresh()
    {
        Refresh();
    }
}
