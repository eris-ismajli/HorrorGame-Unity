using System;
using System.Collections;
using UnityEngine;

public class GirlKitchenLightOff : MonoBehaviour {
    [SerializeField] private Animator creepyGirlAnim;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            creepyGirlAnim.SetTrigger("TurnKitchenLightOff");
            FirstPersonController.Instance.canRun = false;
            FirstPersonController.Instance.speed = 0.3f;
            Destroy(gameObject);
        }
    }
}
