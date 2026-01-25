using System;
using System.Collections;
using UnityEngine;

public class DoorKnock : MonoBehaviour {

    public static event EventHandler OnDoorKnock;
    [SerializeField] private AudioSource dogBarking;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            dogBarking.Play();
            StartCoroutine(WaitBeforeKnocking());
        }
    }

    private IEnumerator WaitBeforeKnocking() {
        yield return new WaitForSeconds(13f);
        OnDoorKnock?.Invoke(this, EventArgs.Empty);
    }
}
