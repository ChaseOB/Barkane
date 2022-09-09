using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalShard : MonoBehaviour
{
    [SerializeField] public Goal goal; 

    public void Collect()
    {
        goal.CollectShard();
        Destroy(this.gameObject);
    }
}
