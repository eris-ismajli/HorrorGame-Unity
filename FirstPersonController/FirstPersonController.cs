using UnityEngine;
using System;
using System.Collections.Generic;

public class FirstPersonController : MonoBehaviour {
    public static FirstPersonController Instance { get; private set; }

    public bool IsRunning { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsCrouching { get; private set; }

    public int knockCounter = 0;

    [Header("Movement")]
    public float speed = 1f;
    public bool canRun = true;
    [SerializeField] private float runSpeed = 2.5f;
    [SerializeField] private KeyCode runningKey = KeyCode.LeftShift;
    [SerializeField] private bool playerCanMove = true;

    [Header("Crouch")]
    [SerializeField] private Transform yawPivot;
    [SerializeField] private Transform pitchPivot;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private bool holdToCrouch = false;       // false = toggle
    [SerializeField] private float crouchSpeed = 0.6f;       // movement speed while crouched
    [SerializeField] private float crouchHeight = 0.9f;      // collider height when crouched
    [SerializeField] private float crouchCamDown = 0.4f;     // how far to lower camera locally
    [SerializeField] private float crouchTransitionTime = 0.12f; // seconds for smooth lerp
    [SerializeField] private LayerMask headObstructionMask = ~0; // what blocks standing up

    [Header("Head Bobbing")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float bobFrequency = 5f;
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float crouchBobMultiplier = 0.6f;

    public List<Func<float>> speedOverrides = new List<Func<float>>();

    private Vector3 defaultCamLocalPos;
    private float bobTimer = 0f;
    private float currentSpeedMagnitude = 0f;

    // crouch internals
    private CharacterController cc;
    private float standHeight;
    private Vector3 standCenter;
    private Vector3 crouchCenter;
    private float heightTarget;
    private Vector3 centerTarget;
    private float crouchLerpT = 1f;

    private Vector3 defaultHeadLocalPos;
    private Vector3 headTargetLocalPos;


    private void Awake() {
        Instance = this;


        if (pitchPivot == null) pitchPivot = cameraTransform.parent;
        if (yawPivot == null) yawPivot = pitchPivot != null ? pitchPivot.parent : null;

        defaultHeadLocalPos = pitchPivot.localPosition;
        defaultCamLocalPos = cameraTransform.localPosition;

        cc = GetComponent<CharacterController>();
        if (cc == null) {
            Debug.LogError("FirstPersonController requires a CharacterController.");
            enabled = false;
            return;
        }

        standHeight = cc.height;
        standCenter = cc.center;

        float heightDelta = standHeight - crouchHeight;
        crouchCenter = standCenter - new Vector3(0f, heightDelta * 0.5f, 0f);

        heightTarget = standHeight;
        centerTarget = standCenter;

        crouchLerpT = 1f;
    }


    private void Update() {
        if (!canCrouch || !playerCanMove) return;

        bool requestCrouch = holdToCrouch ? Input.GetKey(crouchKey)
                                          : (Input.GetKeyDown(crouchKey) ? !IsCrouching : IsCrouching);

        if (holdToCrouch) {
            SetCrouch(requestCrouch);
        }
        else if (Input.GetKeyDown(crouchKey)) {
            SetCrouch(requestCrouch);
        }

        if (crouchLerpT < 1f) {
            crouchLerpT = Mathf.Min(1f, crouchLerpT + (Time.deltaTime / Mathf.Max(0.01f, crouchTransitionTime)));
            cc.height = Mathf.Lerp(cc.height, heightTarget, crouchLerpT);
            cc.center = Vector3.Lerp(cc.center, centerTarget, crouchLerpT);
            pitchPivot.localPosition = Vector3.Lerp(
                pitchPivot.localPosition,
                headTargetLocalPos,
                crouchLerpT * 1.15f
            );

        }
    }

    private void LateUpdate() {
        if (!playerCanMove) return;
        HandleHeadBobbing();
    }

    private void SetCrouch(bool wantCrouch) {
        if (wantCrouch == IsCrouching) return;

        if (!wantCrouch) {
            if (!HasHeadroomToStand()) return;
        }

        IsCrouching = wantCrouch;

        headTargetLocalPos = IsCrouching
            ? defaultHeadLocalPos + Vector3.down * Mathf.Abs(crouchCamDown)
            : defaultHeadLocalPos;

        heightTarget = IsCrouching ? crouchHeight : standHeight;
        centerTarget = IsCrouching ? crouchCenter : standCenter;

        crouchLerpT = 0f;
    }

    private bool HasHeadroomToStand() {
        float radius = cc.radius;
        float skin = cc.skinWidth + 0.01f;

        float standHalf = standHeight * 0.5f;
        Vector3 standCenterWorld = transform.TransformPoint(standCenter);
        Vector3 bottom = standCenterWorld + Vector3.down * (standHalf - radius);
        Vector3 top = standCenterWorld + Vector3.up * (standHalf - radius);

        Collider[] hits = Physics.OverlapCapsule(bottom, top, radius - skin, headObstructionMask, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < hits.Length; i++) {
            if (!hits[i].transform.IsChildOf(transform)) {
                return false; // something above us
            }
        }
        return true;
    }


    private void FixedUpdate() {
        if (!playerCanMove) return;

        IsRunning = canRun && !IsCrouching && Input.GetKey(runningKey);

        float targetSpeed = IsCrouching ? crouchSpeed : (IsRunning ? runSpeed : speed);
        if (speedOverrides.Count > 0) {
            targetSpeed = speedOverrides[speedOverrides.Count - 1]();
        }

        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (input.magnitude > 1f) input.Normalize();
        IsMoving = input.magnitude > 0.1f;

        Vector3 forward = cameraTransform.forward; forward.y = 0f; forward.Normalize();
        Vector3 right = cameraTransform.right; right.y = 0f; right.Normalize();

        Vector3 moveDirection = (right * input.x + forward * input.y);
        Vector3 displacement = moveDirection * targetSpeed * Time.deltaTime;

        displacement.y = -1f * Time.deltaTime;

        if (cc != null) cc.Move(displacement);
        currentSpeedMagnitude = moveDirection.magnitude * targetSpeed;

    }

    private void HandleHeadBobbing() {
        float amp = bobAmplitude * (IsCrouching ? crouchBobMultiplier : 1f);

        if (currentSpeedMagnitude > 0.1f) {
            bobTimer += Time.deltaTime * bobFrequency * currentSpeedMagnitude;
            float yOff = Mathf.Sin(bobTimer) * amp;
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                defaultCamLocalPos + Vector3.up * yOff,
                Time.deltaTime * 12f
            );
        }
        else {
            cameraTransform.localPosition = Vector3.Lerp(
                cameraTransform.localPosition,
                defaultCamLocalPos,
                Time.deltaTime * 8f
            );
            bobTimer = 0f;
        }
    }


    public void EnableCameraMovement(bool enable) {
        YawFollower.Instance.CanRotateYaw(enable);
        PitchFollower.Instance.CanRotatePitch(enable);
    }

    public void EnablePlayerMovement(bool enabled) {
        playerCanMove = enabled;
    }

    public bool PlayerCanMove() {
        return playerCanMove;
    }
}
