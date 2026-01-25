using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SuitcaseAnimator : IsHoverable {

    public static SuitcaseAnimator Instance { get; private set; }

    private Animator suitcaseAnim;
    private bool hasBeenDropped = false;
    private string targetInfo = "The suitcase is locked. Find something to open it with.";
    private Collider suitcaseCollider;

    [SerializeField] private GameObject progressBarRoot;
    [SerializeField] private Image progressBar;
    [SerializeField] private EquipableObjectSO crowbar;
    [SerializeField] private GameObject crowbarObject;
    [SerializeField] private Pickable axe;
    [SerializeField] private Pickable birthdayNote;
    private BoxCollider noteCollider;

    private Animator crowbarAnim;

    private bool dropAnimFinished = false;

    private bool suitcaseOpened = false;

    [SerializeField] private GameObject allLightsOffTrigger;
    [SerializeField] private GameObject plateFallTrigger;

    // progress is in raw units [0..progressMax]
    private float progress = 0f;
    private float progressMax = 10f;

    // decay speed in progress units per second
    [SerializeField] private float decayPerSecond = .5f;

    private void Awake() {
        Instance = this;
        crowbarAnim = crowbarObject.GetComponent<Animator>();
        suitcaseAnim = GetComponent<Animator>();
        suitcaseCollider = GetComponent<Collider>();
        noteCollider = birthdayNote.gameObject.GetComponent<BoxCollider>();
    }

    private void Start() {
        PlayerInventory.Instance.OnEquip += Inventory_OnEquip;
        PlayerPickupController.Instance.OnObjectEquipped += PickupController_OnObjectEquipped;
        progressBar.fillAmount = 0f;
        progressBarRoot.SetActive(false);
        noteCollider.enabled = false;
    }

    private void PickupController_OnObjectEquipped(object sender, PlayerPickupController.OnObjectEquippedEventArgs e) {
        if (e.objectEquipped == axe) {
            StartCoroutine(PickupWhenReady());
            noteCollider.enabled = true;
        }
    }

    private IEnumerator PickupWhenReady() {
        var ppc = PlayerPickupController.Instance;
        yield return new WaitUntil(() => !ppc.IsBusy);
        PlayerPickupController.Instance.TryPick(birthdayNote);
        birthdayNote.SetOriginalPositionAndRotation(birthdayNote.parent.position, birthdayNote.parent.rotation);
    }

    private void Update() {
        // decay raw progress toward 0
        if (suitcaseOpened) return;

        if (progress > 0f) {
            progress -= decayPerSecond * Time.deltaTime;
            if (progress < 0f) progress = 0f;
            progressBar.fillAmount = progress / progressMax;
        }
    }

    private void Inventory_OnEquip(object sender, PlayerInventory.OnEquipEventArgs e) {
        if (e.equippedObject == crowbar) {
            if (hasBeenDropped) {
                targetInfo = "Open suitcase";
                objectHoverInfo = targetInfo;
            }
        }
    }

    public void FinishDropAnim() {
        dropAnimFinished = true;
    }

    private IEnumerator WaitBeforeDisablingCollider() {
        yield return new WaitForSeconds(.6f);
        suitcaseCollider.enabled = false;
        PlayerPickupController.Instance.TryPick(axe);
    }

    protected override void HandleMouseDown() {
        bool hasCrowbar = PlayerInventory.Instance.HasEquipableObject(crowbar);

        if (!hasBeenDropped) {
            // Enable the plate fall trigger after the player drops the suitcase
            plateFallTrigger.SetActive(true);
            suitcaseAnim.SetTrigger("DropSuitcase");
            targetInfo = hasCrowbar ? "Open Suitcase" : "The suitcase is locked. Find something to open it with.";
        }

        hasBeenDropped = true;
        objectHoverInfo = targetInfo;


        if (hasCrowbar && dropAnimFinished) {
            // add one unit per click, clamp, then map to bar
            targetInfo = "";
            objectHoverInfo = targetInfo;

            if (!suitcaseOpened) {
                progressBarRoot.SetActive(true);
                crowbarObject.SetActive(true);
                progress = Mathf.Min(progress + 1f, progressMax);
                progressBar.fillAmount = progress / progressMax;
                crowbarAnim.SetTrigger("Jam");
            }

            if (progress == progressMax) {
                allLightsOffTrigger.SetActive(true);

                suitcaseOpened = true;
                PlayerInventory.Instance.Unequip(crowbar);
                Destroy(crowbarObject);
                progressBarRoot.SetActive(false);
                suitcaseAnim.SetTrigger("OpenSuitcase");
                StartCoroutine(WaitBeforeDisablingCollider());
            }
        }
    }

}
