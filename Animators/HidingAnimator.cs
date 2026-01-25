using UnityEngine;
using System.Collections;
using System;

public class HidingAnimator : IsHoverable {

    [SerializeField] private Transform setupPosition;
    [SerializeField] private FirstPersonController controller;
    [SerializeField] private HidingSpot hidingSpot;

    [SerializeField] private GameObject rightDoor;
    [SerializeField] private GameObject leftDoor;


    private Animator rightDoorAnimator;
    private Animator leftDoorAnimator;

    private CharacterController cc;
    private Animator playerAnimator;
    public static bool isHiding = false;

    private bool isBusy = false;

    public bool hasHided = false;

    public enum HidingSpot {
        Table,
        Closet
    }

    private void Awake() {
        cc = player.GetComponent<CharacterController>();
        playerAnimator = player.GetComponent<Animator>();

        if (hidingSpot == HidingSpot.Closet) {
            rightDoorAnimator = rightDoor.GetComponent<Animator>();
            leftDoorAnimator = leftDoor.GetComponent<Animator>();
        }
    }

    protected override void HandleMouseDown() {
        if (isBusy || PlayerPickupController.Instance.isInspecting) return;
        isBusy = true;
        isHiding = !isHiding;

        if (isHiding) {
            SetupPlayerPosition();
            PlayHidingAnimation();
        }
        else {
            hasHided = true;
            PlayLeavingAnimation();
        }
    }

    private void SetupPlayerPosition() {
        controller.EnablePlayerMovement(false);

        var cc = player.GetComponent<CharacterController>();

        if (cc != null) cc.enabled = false;

        player.SetPositionAndRotation(setupPosition.position, setupPosition.rotation);

        if (cc != null) {
            cc.enabled = true;
            cc.Move(Vector3.zero);
        }
    }

    private void PlayHidingAnimation() {
        string targetHidingAnimation = hidingSpot == HidingSpot.Table ? "HideUnderTheTable" : "HideInTheCloset";
        playerAnimator.applyRootMotion = false;

        if (hidingSpot == HidingSpot.Closet) {
            StartCoroutine(OpenCloseCloset());
            playerAnimator.SetTrigger(targetHidingAnimation);
        }
        else {
            playerAnimator.SetTrigger(targetHidingAnimation);
        }

        StartCoroutine(WaitForAnimationToEnd(targetHidingAnimation, () => {
            isBusy = false;
        }));
    }

    private void PlayLeavingAnimation() {
        string targetLeaveAnimation = hidingSpot == HidingSpot.Table ? "LeaveTable" : "LeaveCloset";
        if (hidingSpot == HidingSpot.Closet) {
            StartCoroutine(OpenCloseCloset());
            playerAnimator.SetTrigger(targetLeaveAnimation);
        }
        else {
            playerAnimator.SetTrigger(targetLeaveAnimation);
        }

        StartCoroutine(WaitForAnimationToEnd(targetLeaveAnimation, () => {
            playerAnimator.SetTrigger("Idle");
            playerAnimator.applyRootMotion = true;
            controller.EnablePlayerMovement(true);
            isBusy = false;
        }));

    }

    private IEnumerator WaitForAnimationToEnd(string stateName, System.Action onComplete) {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);

        // Wait until the correct animation starts
        while (!stateInfo.IsName(stateName)) {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        }

        // Wait until the animation has finished
        while (stateInfo.normalizedTime < 1f) {
            yield return null;
            stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        }

        onComplete?.Invoke();
    }

    private IEnumerator OpenCloseCloset() {
        leftDoorAnimator.SetTrigger("LeftDoorOpen");
        rightDoorAnimator.SetTrigger("RightDoorOpen");

        yield return new WaitForSeconds(2f);

        leftDoorAnimator.SetTrigger("LeftDoorClose");
        rightDoorAnimator.SetTrigger("RightDoorClose");
    }

}
