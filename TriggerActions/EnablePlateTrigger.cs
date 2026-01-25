using UnityEngine;

public class EnablePlateTrigger : MonoBehaviour {
    [SerializeField] private GameObject plateFallTrigger;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            plateFallTrigger.SetActive(true);
            Destroy(gameObject);
        }
    }
}
