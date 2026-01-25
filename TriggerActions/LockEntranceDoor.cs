using UnityEngine;
using System.Collections;

public class LockEntranceDoor : MonoBehaviour {
    [SerializeField] private Door entranceDoor;
    [SerializeField] private AudioSource horrorAtmosphere;

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


    private IEnumerator PrepareAndPlayAmbience() {
        var clip = horrorAtmosphere.clip;
        if (clip == null) yield break;

        // Kick off async load (if not already loaded)
        if (!clip.preloadAudioData && clip.loadState != AudioDataLoadState.Loaded) {
            clip.LoadAudioData();
            while (clip.loadState == AudioDataLoadState.Loading) {
                yield return null; // wait a few frames without blocking
            }
        }

        // Schedule start slightly in the future to avoid sync hitches
        double startTime = AudioSettings.dspTime + 0.05;
        horrorAtmosphere.PlayScheduled(startTime);
    }
}
