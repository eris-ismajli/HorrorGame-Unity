using System.Collections;
using UnityEngine;

public class PeopleTalking : MonoBehaviour {
    [SerializeField] private AudioSource peopleTalking;
    [SerializeField] private AudioSource peopleWalking;
    [SerializeField] private Door peopleDoor;

    [Header("Fade Settings")]
    [SerializeField] private float fadeInDuration = 3f;   // how long it takes to fade in
    [SerializeField] private float fadeOutDelay = 8f;     // wait this many seconds before fading out
    [SerializeField] private float fadeOutDuration = 3f;  // how long the fade-out lasts

    [Header("Door Settings")]
    [SerializeField] private float doorCloseDelay = 9f; // when to close the door (after trigger)

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            // Start both audio sources muted
            peopleTalking.volume = 0f;
            peopleWalking.volume = 0f;

            peopleTalking.Play();
            peopleWalking.Play();

            // Fade in both
            StartCoroutine(FadeIn(peopleTalking));
            StartCoroutine(FadeIn(peopleWalking));

            // Then fade them out after delay
            StartCoroutine(FadeOutAfterDelay(peopleTalking, fadeOutDelay));
            StartCoroutine(FadeOutAfterDelay(peopleWalking, fadeOutDelay));

            // Handle door
            StartCoroutine(WaitBeforeClosingDoor());
        }
    }

    private IEnumerator FadeIn(AudioSource source) {
        float elapsed = 0f;
        while (elapsed < fadeInDuration) {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        source.volume = 1f;
    }

    private IEnumerator FadeOutAfterDelay(AudioSource source, float delay) {
        yield return new WaitForSeconds(delay);

        float startVolume = source.volume;
        float elapsed = 0f;

        while (elapsed < fadeOutDuration) {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        source.volume = 0f;
        source.Stop();
    }

    private IEnumerator WaitBeforeClosingDoor() {
        yield return new WaitForSeconds(doorCloseDelay);
        peopleDoor.ToggleDoor();
        Destroy(peopleWalking.gameObject);
        Destroy(peopleWalking.gameObject);
        Destroy(gameObject);
    }
}
