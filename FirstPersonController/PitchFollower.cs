using UnityEngine;

public class PitchFollower : MonoBehaviour {

    public static PitchFollower Instance { get; private set; }

    [SerializeField] private float cameraSensitivity = 1.0f;
    [SerializeField] private float smoothSpeed = 10f;
    [SerializeField] private float pitchClamp = 90f;

    private float pitch;
    private float currentPitch;

    private bool canRotatePitch = true;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        pitch = transform.localEulerAngles.x;
    }

    private void LateUpdate() {
        if (!canRotatePitch) return;

        float mouseY = Input.GetAxisRaw("Mouse Y");
        pitch -= mouseY * cameraSensitivity;
        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);

        currentPitch = Mathf.LerpAngle(currentPitch, pitch, smoothSpeed * Time.deltaTime);
        transform.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    public void CanRotatePitch(bool canRotate) {
        canRotatePitch = canRotate;
    }
}
