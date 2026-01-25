using UnityEngine;

public class CameraPositionFollower : MonoBehaviour {
    [SerializeField] private Transform target; // FirstPersonController
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float smoothSpeed = 10f;


    void LateUpdate() {
        Vector3 targetPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }

}
