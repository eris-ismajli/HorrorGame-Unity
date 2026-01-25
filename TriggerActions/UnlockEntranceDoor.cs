using UnityEngine;

public class UnlockEntranceDoor : MonoBehaviour {

    [SerializeField] private Door entranceDoor;
    [SerializeField] private GameObject stairWell;
    [SerializeField] private GameObject stairWellFog;

    private Animator doorAnimator;

    private void Awake() {
        doorAnimator = entranceDoor.gameObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        // ✅ centralize through Door; ensure we’re closed so next click opens
        entranceDoor.UnlockFromTriggerKeepClosed();

        stairWell.SetActive(true);
        stairWellFog.SetActive(true);
        Destroy(gameObject);
    }

}
