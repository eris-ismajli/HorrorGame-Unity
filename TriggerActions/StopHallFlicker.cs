using System.Collections;
using UnityEngine;

public class StopHallFlicker : MonoBehaviour {
    [SerializeField] private AnimationFunctions girlAnimationFunctions;
    [SerializeField] private EquipableObjectSO flashlightEquipableSO;

    private float playerSpeed;

    private bool triggered = false;

    private void Start() {
        playerSpeed = FirstPersonController.Instance.speed;
    }

    private void OnTriggerEnter(Collider other) {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;

        SoundManager.Instance.PlayWhisperTrailSound(Camera.main.transform.position, 1f);
        StormVolumeManager.Instance.FadeOutCompletely();

        StartCoroutine(GirlVanish());
    }

    private IEnumerator GirlVanish() {
        if (girlAnimationFunctions != null) {
            girlAnimationFunctions.StopFlicker();


            if (PlayerInventory.Instance.HasEquipableObject(flashlightEquipableSO)) {
                if (FlashlightStatus.Instance.IsFlashlighOn()) {
                    FlashlightStatus.Instance.ToggleFlashlight();
                }
                FlashlightStatus.Instance.CanBeToggled(false);
            }

            yield return new WaitForSeconds(1.5f);

            SafeCodeManager.Instance.canStareAtPlayer = false;
            girlAnimationFunctions.EndGirlAnimation();

            yield return null;

            girlAnimationFunctions.TurnLightsBackOn();

            if (PlayerInventory.Instance.HasEquipableObject(flashlightEquipableSO)) {
                if (!FlashlightStatus.Instance.IsFlashlighOn()) {
                    FlashlightStatus.Instance.ToggleFlashlight();
                }
                FlashlightStatus.Instance.CanBeToggled(true);
            }

            if (DistanceBasedLightFlicker.Instance != null) {
                DistanceBasedLightFlicker.Instance.isGirlVisible = false;
            }

            FirstPersonController.Instance.canRun = true;
            FirstPersonController.Instance.speed = playerSpeed;
        }

        Destroy(gameObject);
    }
}
