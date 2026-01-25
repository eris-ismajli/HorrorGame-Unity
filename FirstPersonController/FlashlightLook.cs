using UnityEngine;

public class FlashlightLook : MonoBehaviour {

    public static FlashlightLook Instance { get; private set; }

    [Header("Flashlight Rotation")]
    [SerializeField] private float flashlightSensitivity = 2.5f;
    [SerializeField] private float pitchClamp = 90f;
    [SerializeField] private float springBackSpeed = 1.0f;
    [SerializeField] private Transform pitchReference;
    [SerializeField] private Transform yawReference;

    [Header("Rotation Fine-Tune (camera-space)")]
    [SerializeField] private Vector3 rotationOffset = Vector3.zero;
    [SerializeField] private bool useCrouchRotationOffset = false;
    [SerializeField] private Vector3 rotationOffsetCrouched = Vector3.zero;
    [SerializeField] private float rotationOffsetLerpSpeed = 12f;

    [Header("Flashlight Position (camera-space follow)")]
    [SerializeField] private Transform positionFollowTarget;
    [SerializeField] private Transform heightReference;
    [SerializeField] private float positionFollowSpeed = 12f;
    public Vector3 localHandOffset = new Vector3(0.18f, -0.08f, 0.32f);

    private float yaw;
    private float pitch;

    private Vector3 currentRotationOffset;

    private bool flashlightCanRotate = true;

    private void Awake() {
        Instance = this;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start() {
        var e = transform.rotation.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        if (heightReference == null) heightReference = pitchReference;
        if (yawReference == null && positionFollowTarget != null) yawReference = positionFollowTarget;

        currentRotationOffset = rotationOffset;
    }

    private void LateUpdate() {

        if (!FlashlightStatus.Instance.IsFlashlighOn()) return;

        if (heightReference != null) {
            Vector3 targetPos = heightReference.TransformPoint(localHandOffset);
            transform.position = Vector3.Lerp(transform.position, targetPos, positionFollowSpeed * Time.deltaTime);
        }
        else if (positionFollowTarget != null) {
            transform.position = Vector3.Lerp(
                transform.position,
                positionFollowTarget.position + new Vector3(0f, 1.5f, 0f),
                positionFollowSpeed * Time.deltaTime
            );
        }

        if (!flashlightCanRotate) return;

        Vector2 mouseDelta = new Vector2(
            Input.GetAxisRaw("Mouse X"),
            Input.GetAxisRaw("Mouse Y")
        );

        yaw += mouseDelta.x * flashlightSensitivity;
        pitch -= mouseDelta.y * flashlightSensitivity;

        // recenter flashlight
        if (yawReference != null) {
            float camYaw = yawReference.rotation.eulerAngles.y;
            yaw = Mathf.LerpAngle(yaw, camYaw, Time.deltaTime * springBackSpeed);
            Debug.Log(yaw);
        }

        if (pitchReference != null) {
            float camPitch = pitchReference.rotation.eulerAngles.x;
            if (camPitch > 180f) camPitch -= 360f;
            camPitch = Mathf.Clamp(camPitch, -pitchClamp, pitchClamp);
            pitch = Mathf.Lerp(pitch, camPitch, Time.deltaTime * springBackSpeed);
        }
        //---

        pitch = Mathf.Clamp(pitch, -pitchClamp, pitchClamp);


        Vector3 targetOffset = rotationOffset;
        if (useCrouchRotationOffset && FirstPersonController.Instance != null && FirstPersonController.Instance.IsCrouching) {
            targetOffset = rotationOffsetCrouched;
        }

        currentRotationOffset = Vector3.Lerp(currentRotationOffset, targetOffset, rotationOffsetLerpSpeed * Time.deltaTime);

        Quaternion baseRot = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion tweak = Quaternion.Euler(currentRotationOffset);
        transform.rotation = baseRot * tweak;
    }

    public void EnableFlashlightRotation(bool enable) {
        flashlightCanRotate = enable;
    }

    public void SnapFlashlightToCenter() {
        if (yawReference != null) {
            yaw = yawReference.rotation.eulerAngles.y;
        }

        if (pitchReference != null) {
            float camPitch = pitchReference.rotation.eulerAngles.x;
            if (camPitch > 180f) camPitch -= 360f;
            pitch = Mathf.Clamp(camPitch, -pitchClamp, pitchClamp);
        }

        // Apply rotation immediately
        Vector3 targetOffset = rotationOffset;
        if (useCrouchRotationOffset && FirstPersonController.Instance != null && FirstPersonController.Instance.IsCrouching) {
            targetOffset = rotationOffsetCrouched;
        }

        currentRotationOffset = targetOffset;

        Quaternion baseRot = Quaternion.Euler(pitch, yaw, 0f);
        Quaternion tweak = Quaternion.Euler(currentRotationOffset);
        transform.rotation = baseRot * tweak;
    }


}
