using System;
using UnityEngine;

public class StopMugSlide : MonoBehaviour {

    public event EventHandler OnStopMugSlide;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            OnStopMugSlide?.Invoke(this, EventArgs.Empty);
            Destroy(gameObject);
        }
    }
}
