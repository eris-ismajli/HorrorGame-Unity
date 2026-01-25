using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BreakPlank : IsHoverable {
    [SerializeField]
    private EquipableObjectSO axeEquipableObjectSO;

    public event EventHandler OnPlankBreak;
    public static event EventHandler OnPlankHit;
    public static event EventHandler OnPlankBreakSound;

    [Header("Camera / Ray")]
    [SerializeField]
    private Camera cam;

    [SerializeField]
    private float clickRayDistance = 5f;

    [SerializeField]
    private float surfaceOffset = 0.01f; // used as a small Z push

    [SerializeField]
    private float forwardOffset = .2f;

    [Header("Axe")]
    [SerializeField]
    private GameObject animatedAxe;
    private Animator axeAnimator;

    [Header("Plank")]
    [SerializeField]
    private GameObject brokenPlank;

    [SerializeField]
    private GameObject[] plankPieces;

    [Header("UI (World Space)")]
    [SerializeField]
    private GameObject progressCircleRoot;

    [SerializeField]
    private Image progressCircle;

    [SerializeField]
    private float fillLerpSpeed = 6f;

    [SerializeField]
    private Transform axeVerticalPosition;

    // --- POLISH: tween settings ---
    [Header("Polish (Smoothing)")]
    [SerializeField]
    private float axeTweenDuration = 0.075f; // 60�100ms feels snappy

    [SerializeField]
    private float uiTweenDuration = 0.06f; // match or slightly faster

    [SerializeField]
    private bool smoothUIZ = true;

    [SerializeField, Range(0f, 1f)]
    private float easeSmooth = 1f; // 0=linear, 1=smoothstep
    private Coroutine axeTweenCo;
    private Coroutine uiTweenCo;

    private Rigidbody pieceRB;
    private MeshRenderer plankRenderer;
    private BoxCollider plankCollider;

    private float progress = 0f;

    [SerializeField]
    private float progressMax = 5f;

    private float displayedFill = 0f;
    private float targetFill = 0f;
    private bool shouldBreak = false;

    private void Awake() {
        if (!cam)
            cam = Camera.main;
        axeAnimator = animatedAxe.GetComponent<Animator>();
        if (axeAnimator) {
            axeAnimator.applyRootMotion = false; // make sure animation doesn't move the object
            axeAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        }
        plankCollider = GetComponent<BoxCollider>();
        plankRenderer = GetComponent<MeshRenderer>();
    }

    private void Start() {
        brokenPlank.SetActive(false);
        animatedAxe.SetActive(false);

        displayedFill = 0f;
        targetFill = 0f;
        progressCircle.fillAmount = 0f;
        progressCircleRoot.SetActive(false);
    }

    private void Update() {
        if (!progressCircleRoot.activeSelf)
            return;

        displayedFill = Mathf.MoveTowards(
            displayedFill,
            targetFill,
            fillLerpSpeed * Time.deltaTime
        );
        progressCircle.fillAmount = displayedFill;

        if (shouldBreak && Mathf.Approximately(displayedFill, 1f)) {
            DestroyPlank();
            progressCircleRoot.SetActive(false);
        }
    }

    protected override void HandleMouseDown() {
        if (!PlayerInventory.Instance.HasEquipableObject(axeEquipableObjectSO)) {
            FeedbackUIManager.Instance.ShowFeedback("Nothing to break the planks with");
            return;
        }

        if (!progressCircleRoot.activeSelf)
            progressCircleRoot.SetActive(true);

        // Snap X; Y+Z will be smoothed
        Vector3 basePos = animatedAxe.transform.position;
        basePos.x = transform.position.x + forwardOffset;

        float targetY = axeVerticalPosition ? axeVerticalPosition.position.y : basePos.y;
        float targetZ = basePos.z;

        if (TryGetMouseHitOnThis(out RaycastHit hit)) {
            targetZ = hit.point.z + surfaceOffset;
        }

        // Start smooth tween for the axe (Y+Z together)
        if (axeTweenCo != null)
            StopCoroutine(axeTweenCo);
        axeTweenCo = StartCoroutine(TweenAxeTo(basePos, targetY, targetZ, axeTweenDuration));

        // UI Z (optional smooth)
        if (smoothUIZ) {
            if (uiTweenCo != null)
                StopCoroutine(uiTweenCo);
            uiTweenCo = StartCoroutine(
                TweenUITo(progressCircleRoot.transform.position, targetZ, uiTweenDuration)
            );
        }
        else {
            Vector3 up = progressCircleRoot.transform.position;
            up.z = targetZ;
            progressCircleRoot.transform.position = up;
        }

        animatedAxe.SetActive(true);
        axeAnimator.SetTrigger("AxeHit");
        OnPlankHit?.Invoke(this, EventArgs.Empty);

        progress = Mathf.Min(progress + 1f, progressMax);
        targetFill = progress / progressMax;

        if (progress >= progressMax) {
            shouldBreak = true;
            targetFill = 1f;
        }
    }

    private bool TryGetMouseHitOnThis(out RaycastHit hit) {
        hit = default;
        if (!cam)
            return false;

        // Just use raw Input.mousePosition when DLSS or dynamic res is on
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (plankCollider && plankCollider.Raycast(ray, out hit, clickRayDistance))
            return true;
        return Physics.Raycast(ray, out hit, clickRayDistance);
    }

    // === POLISH: Smooth Y+Z (no overshoot, exact landing) ===
    private IEnumerator TweenAxeTo(Vector3 current, float targetY, float targetZ, float duration) {
        float fromY = current.y;
        float fromZ = current.z;

        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            float e = (easeSmooth > 0f) ? Mathf.SmoothStep(0f, 1f, a) : a;

            current.y = Mathf.LerpUnclamped(fromY, targetY, e);
            current.z = Mathf.LerpUnclamped(fromZ, targetZ, e);
            if (animatedAxe != null) {
                animatedAxe.transform.position = current; // write directly; animator root motion is off
            }
            yield return null;
        }
        current.y = targetY;
        current.z = targetZ;
        if (animatedAxe != null) {
            animatedAxe.transform.position = current;
        }
    }

    // === POLISH: Smooth UI Z for matching feel ===
    private IEnumerator TweenUITo(Vector3 current, float targetZ, float duration) {
        float fromZ = current.z;
        float t = 0f;
        while (t < duration) {
            t += Time.deltaTime;
            float a = Mathf.Clamp01(t / duration);
            float e = (easeSmooth > 0f) ? Mathf.SmoothStep(0f, 1f, a) : a;

            current.z = Mathf.LerpUnclamped(fromZ, targetZ, e);
            progressCircleRoot.transform.position = current;
            yield return null;
        }
        current.z = targetZ;
        progressCircleRoot.transform.position = current;
    }

    private void DestroyPlank() {
        OnPlankBreak?.Invoke(this, EventArgs.Empty);
        OnPlankBreakSound?.Invoke(this, EventArgs.Empty);

        plankCollider.enabled = false;
        plankRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        plankRenderer.receiveShadows = false;
        plankRenderer.enabled = false;
        brokenPlank.SetActive(true);

        foreach (GameObject piece in plankPieces) {
            pieceRB = piece.GetComponent<Rigidbody>();
            pieceRB.isKinematic = false;
            pieceRB.useGravity = true;
            pieceRB.AddForce(transform.up * -1.3f, ForceMode.Impulse);
        }
    }
}
