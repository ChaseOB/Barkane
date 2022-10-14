using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalShard : MonoBehaviour
{
    [SerializeField] public Goal goal; 
    [SerializeField] private GameObject model;

    private Vector3 startPos;
    public float displacement = 0.1f;
    public float hoverSpeed = 1.0f;
    public float rotateSpeed = 1.0f;
    private Vector3 prevVal = Vector3.zero;

    private void Start() {
        startPos = model.transform.position;
        goal = FindObjectOfType<Goal>();
    }
    private void Update() {
        Vector3 currentVal = new Vector3(0, displacement * Mathf.Sin(Mathf.PI * hoverSpeed * Time.time));
        model.transform.localPosition += currentVal - prevVal;
        prevVal = currentVal;
        model.transform.Rotate(Vector3.up, rotateSpeed * 0.1f);
    }




    public void Collect()
    {
        goal.CollectShard();
        Destroy(this.gameObject);
    }
}
