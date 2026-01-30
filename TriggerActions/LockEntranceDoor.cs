using UnityEngine;
using System.Collections;

public class LockEntranceDoor : MonoBehaviour {
    [SerializeField] private Door entranceDoor;

    private GameObject entranceDoorObject;
    private Animator entranceDoorAnim;
    private bool triggered;

    private void Awake() {
        entranceDoorObject = entranceDoor.gameObject;
        entranceDoorAnim = entranceDoorObject.GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other) {
        if (triggered || !other.CompareTag("Player")) return;
        triggered = true;

        GameStateManager.Instance.SetGameState(
            GameStateManager.CurrentGameState.Middle
        );

        if (!entranceDoor.IsDoorOpen()) {
            // If the door is already closed by the time we walk through the trigger
            // make sure to disable the stairwell
            entranceDoor.OnEntranceDoorClosed();
        }
        entranceDoor.ForceCloseAndLockByTrigger();
    }

}
