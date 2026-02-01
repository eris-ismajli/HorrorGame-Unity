using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HoverManager : MonoBehaviour {
    [Header("Raycast")]
    [SerializeField] private Camera cam;

    [Tooltip("Layers that are considered interactable (objects that implement IsHoverable).")]
    [SerializeField] private LayerMask interactableMask;

    [Tooltip("Layers that can block/occlude interaction (usually Default, Environment, Interactable, etc).")]
    [SerializeField] private LayerMask occluderMask = ~0;

    [SerializeField] private float maxRayDistance = 5f;

    [Header("UI")]
    [SerializeField] private Image reticle;
    [SerializeField] private Vector2 hoverSize = new Vector2(100, 100);
    [SerializeField] private Vector2 originalSize = new Vector2(33, 33);
    [SerializeField] private GameObject hoverInfoUI;
    [SerializeField] private TextMeshProUGUI hoverInfoText;

    [Header("Misc")]
    [SerializeField] private LightManager kitchenLight;

    [Header("Debug")]
    [SerializeField] private bool debugDrawRay = false;

    // For TV specific hover management
    [SerializeField] private EquipableObjectSO tvRemoteEquipableSO;

    private IsHoverable currentHover;

    // For a locked FPS crosshair: we raycast from the screen center every frame.
    private Vector3 ScreenCenter => new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0f);

    private void Awake() {
        if (!cam) cam = Camera.main;

        if (hoverInfoUI) hoverInfoUI.SetActive(false);
        if (reticle) reticle.rectTransform.sizeDelta = originalSize;
    }

    private void Start() {
        if (FeedbackUIManager.Instance != null)
            FeedbackUIManager.Instance.OnHoverReactivate += Feedback_OnHoverReactivate;

        Pickable.OnTvRemoteEquipped += TVremote_OnEquipped;
    }

    private void OnDestroy() {
        if (FeedbackUIManager.Instance != null)
            FeedbackUIManager.Instance.OnHoverReactivate -= Feedback_OnHoverReactivate;
    }

    private void Feedback_OnHoverReactivate(object sender, EventArgs e) {
        RefreshHover();
    }

    private void TVremote_OnEquipped(object sender, EventArgs e) {
        RefreshHover();
    }

    private void RefreshHover() {
        if (currentHover != null) {
            OnHoverEnter(currentHover);
        }
    }

    private void Update() {
        if (!cam) return;

        // Accurate for HDRP + DLSS/Dynamic Resolution: use Unity's ScreenPointToRay.
        // DO NOT do manual viewport math; it can drift when camera pixelRect / scaling changes.
        Ray ray = cam.ScreenPointToRay(ScreenCenter);

        if (debugDrawRay)
            Debug.DrawRay(ray.origin, ray.direction * maxRayDistance, Color.green);

        IsHoverable newHover = TryHitInteractable(ray);

        // if (newHover == null)
        //     TryTriggerSecondary(ray);

        if (newHover != currentHover) {
            if (currentHover != null) OnHoverExit();
            currentHover = newHover;
            if (currentHover != null) OnHoverEnter(currentHover);
        }

        // Safety: if something changed size without exit, force exit.
        if (currentHover == null && reticle && reticle.rectTransform.sizeDelta == hoverSize)
            OnHoverExit();

        if (currentHover != null && Input.GetMouseButtonDown(0))
            currentHover.ClickFromRaycaster();
    }

    private IsHoverable TryHitInteractable(Ray ray) {
        // We raycast against "occluders" so the FIRST thing in front of the player is what counts.
        // That makes interaction feel correct (walls block objects behind them).
        // Make sure your occluderMask includes environment + interactables.
        if (!Physics.Raycast(ray, out RaycastHit hit, maxRayDistance, occluderMask, QueryTriggerInteraction.Ignore))
            return null;

        // Only accept the hit if it's on an interactable layer.
        if (((1 << hit.collider.gameObject.layer) & interactableMask.value) == 0)
            return null;

        IsHoverable hov = hit.collider.GetComponentInParent<IsHoverable>();
        if (hov == null)
            return null;

        float maxDist = hov.MaxInteractionDistance;
        if (hit.distance > maxDist)
            return null;

        // Optional distance check vs player transform if your IsHoverable uses that pattern.
        if (hov.player != null && Vector3.Distance(hov.transform.position, hov.player.position) > maxDist)
            return null;

        return hov;
    }

    // Secondary ray: triggers / scripted targets.
    // private void TryTriggerSecondary(Ray ray)
    // {
    //     // Use same screen-center ray, but you can allow triggers here.
    //     if (!Physics.Raycast(ray, out RaycastHit hit, 5f, occluderMask, QueryTriggerInteraction.Collide))
    //         return;

    // }

    private void OnHoverEnter(IsHoverable hov) {
        if (!hov.isInteractable) return;

        if (reticle) reticle.rectTransform.sizeDelta = hoverSize;

        if (PlayerPickupController.Instance != null && PlayerPickupController.Instance.isInspecting)
            return;

        if (FeedbackUIManager.Instance != null && FeedbackUIManager.Instance.feedbackIsActive)
            return;

        if (!hoverInfoUI) return;

        hoverInfoUI.SetActive(true);

        if (!hoverInfoText) return;

        if (hov is HidingAnimator hidingSpot) {
            bool firstTimeHiding = !hidingSpot.hasHided;
            if (firstTimeHiding)
                hoverInfoText.text = HidingAnimator.isHiding ? "Get out" : "Hide";
            else
                hoverInfoText.text = "";
        }
        else {
            hoverInfoText.text = hov.ObjectHoverInfo;
        }
    }

    private void OnHoverExit() {
        if (hoverInfoUI) {
            hoverInfoUI.SetActive(false);
            if (hoverInfoText) hoverInfoText.text = "";
        }

        if (reticle) reticle.rectTransform.sizeDelta = originalSize;
    }
}
