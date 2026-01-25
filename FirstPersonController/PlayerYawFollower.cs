using UnityEngine;

public class PlayerYawFollower : MonoBehaviour {

    [SerializeField] private Transform yawSource; // Assign the YawFollower here!

    private void LateUpdate() {
        if (yawSource == null) return;

        Vector3 currentEuler = transform.eulerAngles;
        Vector3 sourceEuler = yawSource.eulerAngles;

        // Copy only the Y rotation (horizontal)
        transform.rotation = Quaternion.Euler(currentEuler.x, sourceEuler.y, currentEuler.z);
    }
}
