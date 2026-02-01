
using System;
using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Pickable : IsHoverable {

    public static event EventHandler<OnPickedEventArgs> OnPicked;

    public class OnPickedEventArgs : EventArgs {
        public string objectPickedName;
    }

    public static event EventHandler OnTvRemoteEquipped;

    [SerializeField] private Transform keyStart;
    [SerializeField] private EquipableObjectSO equipableObject;
    [SerializeField] private GameObject objectInfoUI;
    [SerializeField] private TextMeshProUGUI objectInfoTextUI;
    [SerializeField] private string objectInfoText;
    [SerializeField] private bool isEquipable;

    [SerializeField] private string objectPickedName;

    [SerializeField] private GameObject crawlTrigger;

    [SerializeField] private LightSwitchManager hallLightSwitch;
    [SerializeField] private LightSwitchManager kitchenLightSwitch;

    public AudioClip pickUpSound = null;

    public Transform parent;

    private Vector3 origPos;
    private Quaternion origRot;

    public bool isOnToilet = false;

    // For the drawer key to help with collider issues
    [SerializeField] private DrawerAnimator drawer;
    [SerializeField] private bool isDrawerKey = false;
    private BoxCollider drawerCollider;
    //

    // For automatically picking up the padlock key after inspecting the secret note
    [SerializeField] private bool isSecretNote = false;
    [SerializeField] private Pickable padlockKey;
    //


    private void Awake() {
        origPos = transform.position;
        origRot = transform.rotation;

        if (drawer != null) {
            drawerCollider = drawer.gameObject.GetComponent<BoxCollider>();
        }
    }

    private void Start() {
        PlayerInventory.Instance.OnEquip += Inventory_OnEquip;

        if (equipableObject != null) {
            objectPickedName = equipableObject.objectName;
        }
    }

    private void Inventory_OnEquip(object sender, PlayerInventory.OnEquipEventArgs e) {
        if (padlockKey != null) {
            if (e.equippedObject == padlockKey.equipableObject) {
                this.objectHoverInfo = "";
                StartCoroutine(WaitBeforeShowingTask());
                GameStateManager.Instance.SetGameState(
                    GameStateManager.CurrentGameState.Ending
                );
            }
        }
    }

    private IEnumerator WaitBeforeShowingTask() {
        yield return new WaitForSeconds(.8f);
        FeedbackUIManager.Instance.ShowCustomDelayedFeedback("Get out of the apartment and find out what the key unlocks.", 4.5f);
    }

    public void Pick(Transform holdParent) {

        if (TryGetComponent<Rigidbody>(out var rb)) {

            objectInfoUI.SetActive(true);
            objectInfoTextUI.text = objectInfoText;

            OnPicked?.Invoke(this, new OnPickedEventArgs {
                objectPickedName = objectPickedName
            });

            rb.isKinematic = true;
            rb.useGravity = true;

            transform.SetParent(holdParent, worldPositionStays: true);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            foreach (var col in GetComponentsInChildren<Collider>()) {
                col.enabled = false;
            }

        }
    }

    private IEnumerator PickupKeyWhenReady() {
        yield return new WaitUntil(() => !PlayerPickupController.Instance.IsBusy);
        PlayerPickupController.Instance.TryPick(padlockKey);
    }

    public void Take() {

        if (isOnToilet) {
            if (crawlTrigger != null) {
                crawlTrigger.SetActive(true);
            }
        }

        objectInfoUI.SetActive(false);
        objectInfoTextUI.text = "";

        if (!isEquipable) {

            if (isSecretNote) {
                if (padlockKey != null && !PlayerInventory.Instance.HasEquipableObject(padlockKey.equipableObject)) {
                    StartCoroutine(PickupKeyWhenReady());
                }
            }

            transform.SetParent(parent);
            transform.position = origPos;
            transform.rotation = origRot;

            foreach (var col in GetComponentsInChildren<Collider>()) {
                col.enabled = true;
            }

            return;
        }

        var rb = GetComponent<Rigidbody>();
        if (rb != null) {
            Destroy(rb);
        }

        PlayerInventory.Instance.Equip(equipableObject);

        if (equipableObject.objectName == "Storage Key") {
            if (isDrawerKey && drawerCollider != null) {
                drawerCollider.enabled = true;
                drawer.drawerHasObject = false;
            }
        }
        else if (equipableObject.objectName == "Flashlight") {
            FlashlightStatus.Instance.CanBeToggled(true);
            FlashlightStatus.Instance.ToggleFlashlight();
            FeedbackUIManager.Instance.ShowCustomDelayedFeedback("'F' to toggle flashlight");
        }
        else if (equipableObject.objectName == "TV Remote") {
            OnTvRemoteEquipped?.Invoke(this, EventArgs.Empty);
            FeedbackUIManager.Instance.ShowCustomDelayedFeedback("Click the TV to toggle it on and off", 3f);

        }

        gameObject.SetActive(false);

        transform.SetParent(keyStart);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;


    }

    public void SetOriginalPositionAndRotation(Vector3 newOrigPos, Quaternion newOrigRot) {
        origPos = newOrigPos;
        origRot = newOrigRot;
    }
}
