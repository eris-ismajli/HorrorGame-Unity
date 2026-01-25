using UnityEngine;

public class YawFollower : MonoBehaviour {

    public static YawFollower Instance { get; private set; }

    [SerializeField] private float cameraSensitivity = 1.0f;
    [SerializeField] private float smoothSpeed = 10f;

    private float yaw;
    private float currentYaw;

    private bool canRotateYaw = true;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        yaw = transform.eulerAngles.y;
    }

    private void LateUpdate() {
        if (!canRotateYaw) return;

        float mouseX = Input.GetAxisRaw("Mouse X");
        yaw += mouseX * cameraSensitivity;

        currentYaw = Mathf.LerpAngle(currentYaw, yaw, smoothSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, currentYaw, 0f);
    }

    public void CanRotateYaw(bool canRotate) {
        canRotateYaw = canRotate;
    }
}
