using System.Collections;
using UnityEngine;

public class WrongDoor : IsHoverable {

    [SerializeField] private AudioSource manShouting;
    [SerializeField] private AudioSource russianWoman;

    private bool isBusy = false;

    protected override void HandleMouseDown() {
        if (isBusy) return;

        StartCoroutine(KnockingCooldown());
        SoundManager.Instance.PlayPlayerKnockingSound(transform.position, .7f);

        FirstPersonController.Instance.knockCounter++;

        if (FirstPersonController.Instance.knockCounter == 1) {
            if (manShouting != null) {
                StartCoroutine(WaitBeforePlayingSound(manShouting, 1.05f));
            }
        }
        else if (FirstPersonController.Instance.knockCounter == 4) {
            if (russianWoman != null) {
                StartCoroutine(WaitBeforePlayingSound(russianWoman, 1.02f));
            }
        }

    }

    private IEnumerator WaitBeforePlayingSound(AudioSource sound, float delay) {
        yield return new WaitForSeconds(delay);
        sound.Play();
    }

    private IEnumerator KnockingCooldown() {
        isBusy = true;
        yield return new WaitForSeconds(2f);
        isBusy = false;
    }

}
