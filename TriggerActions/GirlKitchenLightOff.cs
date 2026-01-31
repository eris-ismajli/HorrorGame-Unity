using System;
using UnityEngine;

public class GirlKitchenLightOff : MonoBehaviour {
    [SerializeField] private Animator creepyGirlAnim;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            creepyGirlAnim.SetTrigger("TurnKitchenLightOff");
            Destroy(gameObject);
        }
    }
}
