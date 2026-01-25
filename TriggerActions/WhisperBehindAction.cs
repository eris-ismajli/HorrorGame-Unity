using UnityEngine;

public class WhisperBehindAction : MonoBehaviour {

    [SerializeField] private Transform whisperSource;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            SoundManager.Instance.PlayWhisperTrailSound(whisperSource.position, 1f);
            StormVolumeManager.Instance.FadeOutCompletely();
            Destroy(gameObject);
        }
    }
}
