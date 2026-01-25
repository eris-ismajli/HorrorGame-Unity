using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System;  // for Canvas.ForceUpdateCanvases()

public class Door : IsHoverable {

    public static event EventHandler OnDoorChanged;
    public static event EventHandler OnDoorLocked;
    public static event EventHandler OnFridgeOpen;

    public bool lockedByTrigger = false;

    [SerializeField] private bool locked = false;
    [SerializeField] private GameObject key;
    [SerializeField] private Transform keyHole;
    [SerializeField] private EquipableObjectSO requiredKey;
    [SerializeField] private bool isFridge = false;
    [SerializeField] private bool isQuietDoor = false;
    [SerializeField] private bool isFullySilentDoor = false;
    [SerializeField] private bool isEntranceDoor = false;
    [SerializeField] private bool isLocker = false;

    [SerializeField] private GameObject StairWell;
    [SerializeField] private GameObject StairWellFog;
    [SerializeField] private GameObject EntranceDoorTrigger;

    private KeyAnimator keyAnim;
    private Animator animator;
    private bool isDoorOpen = false;
    private bool _isBusy;
    private bool doorToggled = false;

    [SerializeField] private EquipableObjectSO padlockKey;
    [SerializeField] private GameObject animatedPadlockKey;
    [SerializeField] private GameObject padlock;
    private Animator padlockKeyAnim;
    private Animator padlockAnim;


    private void Awake() {
        animator = GetComponent<Animator>();
        if (animatedPadlockKey != null && padlock != null) {
            padlockKeyAnim = animatedPadlockKey.GetComponent<Animator>();
            padlockAnim = padlock.GetComponent<Animator>();
        }
        if (key != null) {
            keyAnim = key.GetComponent<KeyAnimator>();
        }
    }

    private void Start() {
        PadlockKeyAnim.OnLockerKeyPullout += PadlockKeyAnim_OnLockerKeyPullout;
    }

    private void PadlockKeyAnim_OnLockerKeyPullout(object sender, EventArgs e) {
        padlockAnim.SetTrigger("OpenPadlock");
        Destroy(animatedPadlockKey);
    }

    protected override void HandleMouseDown() {
        if (_isBusy || lockedByTrigger) return;

        if (locked) {
            // If a key *is* required, check inventory…
            if (requiredKey != null) {
                if (!PlayerInventory.Instance.HasAnyKey()) {
                    if (!isLocker) {
                        StartCoroutine(ShowDoorError("This door is locked"));
                    }
                    else {
                        StartCoroutine(ShowDoorError("This locker is locked"));
                    }
                    if (!isFullySilentDoor) {
                        OnDoorLocked?.Invoke(this, EventArgs.Empty);
                    }
                    return;
                }
                else if (PlayerInventory.Instance.HasAnyKey() && !PlayerInventory.Instance.HasEquipableObject(requiredKey)) {
                    StartCoroutine(ShowDoorError("Key doesn't fit"));
                    if (!isFullySilentDoor) {
                        OnDoorLocked?.Invoke(this, EventArgs.Empty);
                    }
                    return;
                }
            }

            // Either no key is required, or we have the right one—unlock & open
            locked = false;

            if (requiredKey != null) {
                key.SetActive(true);
                PlayerInventory.Instance.Unequip(requiredKey);
            }
            if (!isLocker) {
                StartCoroutine(UnlockAndOpen());
            }
            else {
                animatedPadlockKey.SetActive(true);
                padlockKeyAnim.SetTrigger("InsertTurnPullOut");
            }
        }
        else {
            ToggleDoor();
        }
    }

    private IEnumerator ShowDoorError(string msg) {
        _isBusy = true;
        animator.SetTrigger("DoorLocked");
        FeedbackUIManager.Instance.ShowFeedback(msg);
        yield return new WaitForSeconds(3f);
        animator.SetTrigger("DoorClose");
        _isBusy = false;
    }

    private IEnumerator UnlockAndOpen() {
        _isBusy = true;
        yield return keyAnim.InsertAndTurn(keyHole);
        ToggleDoor();
        _isBusy = false;
    }

    public void ToggleDoor() {
        if (_isBusy || lockedByTrigger) return;
        _isBusy = true;

        doorToggled = true;
        isDoorOpen = !isDoorOpen;

        if (!isFridge && !isQuietDoor && !isFullySilentDoor)
            OnDoorChanged?.Invoke(this, EventArgs.Empty);
        if (isFridge && isDoorOpen)
            OnFridgeOpen?.Invoke(this, EventArgs.Empty);

        if (animator) {
            // Clear any stale triggers before setting the new one
            animator.ResetTrigger("DoorOpen");
            animator.ResetTrigger("DoorClose");
            animator.SetTrigger(isDoorOpen ? "DoorOpen" : "DoorClose");
        }

        // Small debounce so rapid clicks can't stack races; tune to your clip length if needed
        StartCoroutine(ClearBusySoon(0.3f));
    }

    private IEnumerator ClearBusySoon(float t) {
        yield return new WaitForSeconds(t);
        _isBusy = false;
    }


    public bool IsDoorOpen() {
        return isDoorOpen;
    }


    // -- These two methods play the sound effect of the door hinge meeting or leaving the frame
    public void TriggerDoorCloseSound() {
        if (!doorToggled || isFullySilentDoor) return;
        SoundManager.Instance.PlayDoorCloseSound(transform.position, .15f);
    }

    public void TriggerDoorOpenSound() {
        if (isFullySilentDoor) return;
        SoundManager.Instance.PlayDoorOpenSound(transform.position, .3f);
    }
    // --

    public void OnEntranceDoorClosed() {
        bool canDeactivateStairwell = isEntranceDoor && GameStateManager.Instance.currentGameState == GameStateManager.CurrentGameState.Middle && !PlayerInventory.Instance.HasEquipableObject(padlockKey);
        if (!canDeactivateStairwell) return;
        StairWell.SetActive(false);
        StairWellFog.SetActive(false);
        Destroy(EntranceDoorTrigger);
    }

    public void ForceCloseAndLockByTrigger() {
        // Lock and hard-sync the internal state with the visual state.
        lockedByTrigger = true;


        // Cancel any pending ops and clean triggers
        StopAllCoroutines();
        _isBusy = false;

        // Make sure our logical flag matches: we're closed now
        isDoorOpen = false;

        if (animator) {
            animator.ResetTrigger("DoorOpen");
            animator.ResetTrigger("DoorClose");
            animator.SetTrigger("DoorClose");
        }
    }

    public void UnlockFromTriggerKeepClosed() {
        // Unlock and ensure the next click will OPEN (we're closed right now).
        lockedByTrigger = false;

        StopAllCoroutines();
        _isBusy = false;

        // IMPORTANT: keep logic in sync so first click opens
        isDoorOpen = false;

        doorToggled = false;

        // if (animator) {
        //     animator.ResetTrigger("DoorOpen");
        //     animator.ResetTrigger("DoorClose");
        //     // Don't fire any trigger here; we stay visually closed.
        // }
    }


}
