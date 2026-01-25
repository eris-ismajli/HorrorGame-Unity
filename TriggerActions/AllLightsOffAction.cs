using System.Collections;
using UnityEngine;

public class AllLightsOffAction : MonoBehaviour {

    [SerializeField] private CircuitBreakerToggle[] circuits;
    [SerializeField] private CircuitBreakerToggle kitchenCircuit;
    [SerializeField] private float minDelay = 0.5f;
    [SerializeField] private float maxDelay = 1f;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other) {
        if (triggered) return;

        if (other.CompareTag("Player")) {
            triggered = true;
            StartCoroutine(ToggleCircuitsWithDelay());
        }
    }

    private IEnumerator ToggleCircuitsWithDelay() {
        foreach (CircuitBreakerToggle circuit in circuits) {
            if (circuit == kitchenCircuit) {
                Debug.Log("Kitchen Circuit");
                continue;
            }
            if (circuit.IsOn()) {
                circuit.ToggleCircuit();

                float delay = Random.Range(minDelay, maxDelay);
                yield return new WaitForSeconds(delay);
            }
        }

        yield return new WaitForSeconds(1.5f);

        if (kitchenCircuit.IsOn()) {
            kitchenCircuit.ToggleCircuit();
        }

        Destroy(gameObject);
    }
}
