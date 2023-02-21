using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSFX : MonoBehaviour
{
    public void PlayStep() {
        AudioManager.Instance.Play("Step");
    }
}
