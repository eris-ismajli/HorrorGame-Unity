using UnityEngine;

public class FootstepSounds : MonoBehaviour {

    private FirstPersonController player;
    private CharacterController characterController;
    private float footstepTimer;
    private float footsterTimerMax = 0.8f;

    private Vector3 bottomOffset; // Cached offset from center to feet

    private void Awake() {
        player = GetComponent<FirstPersonController>();
        characterController = GetComponent<CharacterController>();

        // Cache the offset once
        float yOffset = -(characterController.height / 2f - characterController.radius);
        bottomOffset = new Vector3(0f, yOffset, 0f);
    }

    private void Update() {
        if (!player.PlayerCanMove()) return;

        float volume = player.IsRunning ? 1f : .57f;
        footsterTimerMax = player.IsRunning ? 0.37f : 0.8f;
        footstepTimer -= Time.deltaTime;

        if (footstepTimer < 0f) {
            footstepTimer = footsterTimerMax;

            if (FirstPersonController.Instance.IsMoving) {
                Vector3 footstepPosition = player.transform.position + bottomOffset;
                SoundManager.Instance.PlayFootstepSound(footstepPosition, volume);
            }
        }
    }
}
