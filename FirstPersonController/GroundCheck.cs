using UnityEngine;

[ExecuteInEditMode]
public class GroundCheck : MonoBehaviour {

    [SerializeField] private float distanceThreshold = .15f;
    [SerializeField] private bool isGrounded = true;

    public event System.Action Grounded;

    private const float ORIGIN_OFFSET = .001f;
    private Vector3 raycastOrigin => transform.position + Vector3.up * ORIGIN_OFFSET;
    private float raycastDistance => distanceThreshold * ORIGIN_OFFSET;

    private void LateUpdate() {

        bool isGroundedNow = Physics.Raycast(raycastOrigin, Vector3.down, distanceThreshold * 2);

        if (isGroundedNow && !isGrounded) {
            Grounded?.Invoke();
        }

        isGrounded = isGroundedNow;
    }

    void OnDrawGizmosSelected() {
        // Draw a line in the Editor to show whether we are touching the ground.
        Debug.DrawLine(raycastOrigin, raycastOrigin + Vector3.down * raycastDistance, isGrounded ? Color.white : Color.red);
    }
}
