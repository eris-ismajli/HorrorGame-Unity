using UnityEngine;
using System.Collections;
using System;

public class CameraFocusController : MonoBehaviour {

    public event EventHandler<OnCameraTransitionEventArgs> OnCameraTransition;

    public static event EventHandler OnZoomOutAfterOpening;

    public class OnCameraTransitionEventArgs : EventArgs {
        public bool isZoomed;
    }

    [SerializeField] private Transform safe;
    [SerializeField] private DetectSafeClick safeClickDetector;
    [SerializeField] private float offsetDistance = 2f;
    [SerializeField] private float zoomDuration = 0.6f;
    [SerializeField] private Transform playerCameraJoint;

    private bool isZoomed = false;
    private bool isBusy = false;
    private Coroutine _zoomRoutine;

    // Cache at Awake
    private Vector3 _restLocalPos;
    private Quaternion _restLocalRot;

    private void Start() {
        safeClickDetector.OnSafeClicked += SafeClickDetector_OnSafeClicked;
    }

    private void SafeClickDetector_OnSafeClicked(object sender, System.EventArgs e) {
        if (isBusy) return;

        isZoomed = !isZoomed;

        if (_zoomRoutine != null)
            StopCoroutine(_zoomRoutine);

        isBusy = true;

        if (isZoomed) {
            // Cache current position/rotation BEFORE zooming in
            _restLocalPos = playerCameraJoint.localPosition;
            _restLocalRot = playerCameraJoint.localRotation;

            OnCameraTransition?.Invoke(this, new OnCameraTransitionEventArgs {
                isZoomed = isZoomed
            });

            Vector3 zoomPos = safe.position - safe.forward * offsetDistance;
            Vector3 lookDir = (safe.position - zoomPos);
            lookDir.y = 0f;
            Quaternion zoomRot = Quaternion.LookRotation(lookDir);

            FirstPersonController.Instance.EnablePlayerMovement(false);
            FirstPersonController.Instance.EnableCameraMovement(false);

            _zoomRoutine = StartCoroutine(SmoothZoom(playerCameraJoint.position, zoomPos, playerCameraJoint.rotation, zoomRot));
        }
        else {
            OnCameraTransition?.Invoke(this, new OnCameraTransitionEventArgs {
                isZoomed = isZoomed
            });

            ZoomOutOfSafe();
        }
    }

    public void ZoomOutOfSafe() {
        _zoomRoutine = StartCoroutine(SmoothZoomLocal(
            playerCameraJoint.localPosition,
            _restLocalPos,
            playerCameraJoint.localRotation,
            _restLocalRot,
            () => {
                FirstPersonController.Instance.EnableCameraMovement(true);
                FirstPersonController.Instance.EnablePlayerMovement(true);
                if (SafeCodeManager.Instance.passwordFound) {
                    OnZoomOutAfterOpening?.Invoke(this, EventArgs.Empty);
                }
            }
        ));
    }


    private IEnumerator SmoothZoom(Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot) {
        float elapsed = 0f;
        while (elapsed < zoomDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            playerCameraJoint.position = Vector3.Lerp(fromPos, toPos, t);
            playerCameraJoint.rotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }

        // snap exactly
        playerCameraJoint.position = toPos;
        playerCameraJoint.rotation = toRot;

        if (!isZoomed) {
            //transform.localPosition = _restPos;
            //transform.localRotation = _restRot;

            // No need to set yaw/pitch – the player look system already preserved it

            FirstPersonController.Instance.EnableCameraMovement(true);
            FirstPersonController.Instance.EnablePlayerMovement(true);
        }


        isBusy = false;
    }

    private IEnumerator SmoothZoomLocal(
        Vector3 fromPos,
        Vector3 toPos,
        Quaternion fromRot,
        Quaternion toRot,
        Action onComplete = null
    ) {
        float elapsed = 0f;
        while (elapsed < zoomDuration) {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / zoomDuration);
            playerCameraJoint.localPosition = Vector3.Lerp(fromPos, toPos, t);
            playerCameraJoint.localRotation = Quaternion.Slerp(fromRot, toRot, t);
            yield return null;
        }

        playerCameraJoint.localPosition = toPos;
        playerCameraJoint.localRotation = toRot;

        isBusy = false;

        onComplete?.Invoke(); // Only enable camera AFTER smooth rotation ends
    }



    public bool IsZoomed() {
        return isZoomed;
    }

}
