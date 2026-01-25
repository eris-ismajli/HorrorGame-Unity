using System;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class PlayerPickupController : MonoBehaviour {

    public static PlayerPickupController Instance { get; private set; }

    public event EventHandler<OnObjectEquippedEventArgs> OnObjectEquipped;

    public class OnObjectEquippedEventArgs : EventArgs {
        public Pickable objectEquipped;
    }

    [SerializeField] private Camera cam;

    [Header("Inspect Settings")]
    [SerializeField] private Light inspectLight;
    [SerializeField] private Transform inspectPivot;         // should be parented to the camera
    [SerializeField] private float baseInspectZ = 0.45f;     // where tiny items sit
    [SerializeField] private float sizeToDistance = 0.35f;   // how strongly size pushes it away
    [SerializeField] private float minInspectZ = 0.25f;      // clamp
    [SerializeField] private float maxInspectZ = 1.2f;       // clamp
    [SerializeField] private float inspectLerp = 12f;        // smoothing speed

    [Header("Pickup")]
    [SerializeField] private float pickRange = 3f;
    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private Volume postProcessing;
    [SerializeField] private LayerMask picakbleLayerMask;

    [SerializeField] private EquipableObjectSO flashlightSO;
    [SerializeField] private HDAdditionalLightData flashlightSpotlight;
    private Vector3 defaultFlashlightOffset;

    private DepthOfField dof;
    private Pickable currentPick;

    public bool isInspecting = false;

    // runtime
    private float targetInspectZ;

    private Vector3 ScreenCenter => new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

    private void Awake() {
        Instance = this;
        var profile = postProcessing.profile;
        if (!profile.TryGet<DepthOfField>(out dof)) {
            dof = profile.Add<DepthOfField>(true);
        }
        targetInspectZ = inspectPivot.localPosition.z;
        if (!cam) cam = Camera.main;
    }

    private void Start() {
        defaultFlashlightOffset = FlashlightLook.Instance.localHandOffset;
    }

    private void Update() {

        if (Input.GetMouseButtonDown(0)) {
            if (currentPick == null) {
                TryPick();
            }
            else {
                TakeCurrent();
            }
        }

        if (currentPick != null) {
            HandleRotation();

            // Smoothly move pivot toward target z (local space, forward of camera)
            var lp = inspectPivot.localPosition;
            lp.z = Mathf.Lerp(lp.z, targetInspectZ, 1f - Mathf.Exp(-inspectLerp * Time.deltaTime));
            inspectPivot.localPosition = lp;
        }
    }

    public void TryPick(Pickable currentItem = null) {

        if (currentItem != null) {
            currentPick = currentItem;
            currentPick.Pick(inspectPivot);
            targetInspectZ = ComputeInspectZ(currentPick.transform);
            EnterInspectMode();
            return;
        }

        Ray ray = cam.ScreenPointToRay(ScreenCenter);

        if (Physics.Raycast(ray, out var hit, pickRange)) {
            if (hit.collider.TryGetComponent<Pickable>(out var pickable)) {
                currentPick = pickable;
                currentPick.Pick(inspectPivot);

                // compute distance based on picked object's size
                targetInspectZ = ComputeInspectZ(currentPick.transform);
                EnterInspectMode();
            }
        }
    }

    private void TakeCurrent() {
        currentPick.Take();
        OnObjectEquipped?.Invoke(this, new OnObjectEquippedEventArgs {
            objectEquipped = currentPick
        });
        currentPick = null;
        ExitInspectMode();
    }

    private void HandleRotation() {
        float horiz = Input.GetAxis("Mouse X") * rotationSpeed;
        inspectPivot.Rotate(Vector3.up, horiz, Space.World);

        float vert = Input.GetAxis("Mouse Y") * rotationSpeed;
        inspectPivot.Rotate(Vector3.right, vert, Space.Self);
    }

    private void EnterInspectMode() {
        isInspecting = true;
        dof.active = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = false;
        FirstPersonController.Instance.EnableCameraMovement(false);
        FirstPersonController.Instance.EnablePlayerMovement(false);
        ToggleInspectLighting(true);
    }

    private void ExitInspectMode() {
        isInspecting = false;
        dof.active = false;
        Cursor.lockState = CursorLockMode.Locked;
        FirstPersonController.Instance.EnableCameraMovement(true);
        if (!HidingAnimator.isHiding) {
            FirstPersonController.Instance.EnablePlayerMovement(true);
        }
        ToggleInspectLighting(false);

    }

    private void ToggleInspectLighting(bool enable) {
        inspectLight.enabled = enable;

        // if (!PlayerInventory.Instance.HasEquipableObject(flashlightSO)) return;

        Vector3 inspectingFlashlightOffset = new Vector3(0.2f, 0f, -0.6f);
        Vector3 targetFlashlightOffset = enable ? inspectingFlashlightOffset : defaultFlashlightOffset;

        FlashlightLook.Instance.localHandOffset = targetFlashlightOffset;
        flashlightSpotlight.EnableShadows(!enable);
        FlashlightLook.Instance.SnapFlashlightToCenter();
        FlashlightLook.Instance.EnableFlashlightRotation(!enable);

    }

    // --- Helpers ---

    // Returns a good Z distance based on the object's world-space bounding box.
    private float ComputeInspectZ(Transform t) {
        if (t == null) return baseInspectZ;

        // Gather all renderers (fallback to colliders if no renderers)
        var renderers = t.GetComponentsInChildren<Renderer>(true);
        Bounds b;

        if (renderers.Length > 0) {
            b = new Bounds(renderers[0].bounds.center, Vector3.zero);
            for (int i = 0; i < renderers.Length; i++) b.Encapsulate(renderers[i].bounds);
        }
        else {
            var cols = t.GetComponentsInChildren<Collider>(true);
            if (cols.Length == 0) return Mathf.Clamp(baseInspectZ, minInspectZ, maxInspectZ);
            b = new Bounds(cols[0].bounds.center, Vector3.zero);
            for (int i = 0; i < cols.Length; i++) b.Encapsulate(cols[i].bounds);
        }

        // Use largest dimension as "size"
        float size = Mathf.Max(b.size.x, b.size.y, b.size.z);

        // Map size -> distance
        float z = baseInspectZ + size * sizeToDistance;
        return Mathf.Clamp(z, minInspectZ, maxInspectZ);
    }

    public bool IsBusy => isInspecting || currentPick != null;
}
