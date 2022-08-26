using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Target : MonoBehaviour
{
    //C: Used on triggers for calling events when objects with certain tags enter
    [SerializeField] private List<string> compareTags = new List<string>();

    public UnityEvent OnEnter;
    public UnityEvent OnExit;


    private void OnTriggerEnter(Collider other) {
        foreach (string tag in compareTags) 
            if(other.gameObject.CompareTag(tag))
                OnEnter?.Invoke();
    }
    
    private void OnTriggerExit(Collider other) {
        foreach (string tag in compareTags) 
            if(other.gameObject.CompareTag(tag))
                OnExit?.Invoke();
    }
}
