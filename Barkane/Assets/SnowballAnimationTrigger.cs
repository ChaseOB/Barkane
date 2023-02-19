using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SnowballAnimationTrigger : MonoBehaviour
{
    [SerializeField] private Snowball snowball;
    public LayerMask playerMask;



    private void OnTriggerEnter(Collider other) {
        if(playerMask == (playerMask | (1 << other.gameObject.layer))){
            snowball.OnPlayerCollide();
        }
    }
}
