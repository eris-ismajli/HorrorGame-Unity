using UnityEngine;

public class StormVolumeZone : MonoBehaviour {
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            StormVolumeManager.Instance.EnterZone();
        }
    }

    private void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            StormVolumeManager.Instance.ExitZone();
        }
    }
}
