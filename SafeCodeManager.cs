using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class SafeCodeManager : MonoBehaviour {

    public static SafeCodeManager Instance { get; private set; }

    [SerializeField] private CameraFocusController safeCameraController;
    [SerializeField] private GameObject safeCodeUI;
    [SerializeField] private TextMeshProUGUI codeInstruction;
    [SerializeField] private TextMeshProUGUI playerCodeInputText;
    [SerializeField] private TextMeshProUGUI codeUnderlines;
    [SerializeField] private TextMeshProUGUI tip;
    [SerializeField] private GameObject safeDoor;
    [SerializeField] private GameObject clickDetector;

    [SerializeField] private Pickable calendar;
    [SerializeField] private Pickable birthdayNote;
    [SerializeField] private Transform drawer;
    [SerializeField] private DrawerAnimator drawerAnimator;
    [SerializeField] private Transform calendarPosition;
    [SerializeField] private Transform birthdayNotePosition;

    [SerializeField] private GameObject creepyGirl;
    [SerializeField] private LightManager kitchenLight;
    [SerializeField] private GameObject kitchenLightOffTrigger;
    [SerializeField] private GameObject stopHallFlickerTrigger;
    [SerializeField] private Transform creepyGirlHead;
    [SerializeField] private Transform playerCamera;
    public bool canStareAtPlayer = false;

    private Animator creepyGirlAnim;

    private int interactionCounter = 0;

    private string SAFE_CODE = "09031970";
    private string playerCodeInput = "";
    private string TIP_TEXT = "MMDDYYYY";
    private int codeMaxCharacters = 8;

    private int incorrectGuesses = 0;
    private int incorrectGuessesBeforeTip = 1;

    private Animator safeDoorOpenAnim;

    public bool InteractedWithSafe = false;

    private bool hasFailed = false;
    public bool passwordFound = false;

    private void Awake() {
        Instance = this;
        safeDoorOpenAnim = safeDoor.GetComponent<Animator>();
        creepyGirlAnim = creepyGirl.GetComponent<Animator>();
    }

    private void Start() {
        safeCameraController.OnCameraTransition += SafeCameraController_OnCameraTransition;
    }


    private void Update() {
        if (!safeCameraController.IsZoomed()) return;

        // Type numbers only; no delete/backspace allowed.
        for (int i = 0; i <= 9; i++) {
            if (Input.GetKeyDown(KeyCode.Alpha0 + i) || Input.GetKeyDown(KeyCode.Keypad0 + i)) {
                codeUnderlines.color = Color.white;

                if (playerCodeInput.Length >= codeMaxCharacters) break;

                if (codeInstruction.gameObject.activeSelf)
                    codeInstruction.gameObject.SetActive(false);

                if (!playerCodeInputText.gameObject.activeSelf)
                    playerCodeInputText.gameObject.SetActive(true);

                if (!codeUnderlines.gameObject.activeSelf)
                    codeUnderlines.gameObject.SetActive(true);

                playerCodeInput += i.ToString();
                playerCodeInputText.text = playerCodeInput;

                UpdateTip(typing: true);

                if (playerCodeInput.Length == codeMaxCharacters) {
                    // The player entered the correct password
                    if (playerCodeInput == SAFE_CODE) {
                        passwordFound = true;
                        safeCodeUI.SetActive(false);
                        safeDoorOpenAnim.SetTrigger("SafeDoorOpen");
                        Destroy(clickDetector);
                        StartCoroutine(WaitBeforeZoomingOut());
                    }
                    else {
                        hasFailed = true;

                        codeUnderlines.color = Color.red;
                        incorrectGuesses++;

                        if (incorrectGuesses >= incorrectGuessesBeforeTip) {
                            tip.gameObject.SetActive(true);
                            tip.text = TIP_TEXT;
                        }
                    }

                    // lock-in behavior: once full, clear for next attempt (still no delete mid-entry)
                    playerCodeInput = "";
                    playerCodeInputText.text = playerCodeInput;
                    UpdateTip(typing: false);
                }

                break;
            }
        }
    }

    private void LateUpdate() {
       if (!canStareAtPlayer) return;
       creepyGirlHead.LookAt(playerCamera); 
    }

    private void UpdateTip(bool typing) {
        if (!hasFailed) return;
        if (typing) {
            if (!string.IsNullOrEmpty(TIP_TEXT)) {
                TIP_TEXT = TIP_TEXT.Remove(0, 1);
                tip.text = TIP_TEXT;
            }
        }
        else {
            TIP_TEXT = "MMDDYYYY";
            tip.text = TIP_TEXT;
        }
    }


    // This function runs after the player enters the correct password
    private IEnumerator WaitBeforeZoomingOut() {
        yield return new WaitForSeconds(3f);
        safeCameraController.ZoomOutOfSafe();
        // Destroy(gameObject);
    }

    private void SafeCameraController_OnCameraTransition(object sender, CameraFocusController.OnCameraTransitionEventArgs e) {
        safeCodeUI.SetActive(e.isZoomed);

        interactionCounter++;

        if (interactionCounter == 1) {
            InteractedWithSafe = true;
            ChangeCalendarAndNotePosition();

            EnableTrigger(kitchenLightOffTrigger);
            EnableTrigger(stopHallFlickerTrigger);
            creepyGirlAnim.SetTrigger("CreepyPeak");
            canStareAtPlayer = true;
            if (!kitchenLight.IsThisLightOn()) {
                kitchenLight.ToggleLight(true);
            }
        }
    }

    private void EnableTrigger(GameObject trigger) {
        if (trigger != null && trigger.activeSelf == false) {
            trigger.SetActive(true);
        }
    }

    private void ChangeCalendarAndNotePosition() {
        calendar.transform.SetParent(drawer);
        calendar.transform.SetPositionAndRotation(calendarPosition.position, calendarPosition.rotation);
        calendar.SetOriginalPositionAndRotation(calendarPosition.position, calendarPosition.rotation);
        drawerAnimator.drawerHasObject = true;
        calendar.parent = drawer;

        birthdayNote.transform.SetPositionAndRotation(birthdayNotePosition.position, birthdayNotePosition.rotation);
        birthdayNote.SetOriginalPositionAndRotation(birthdayNotePosition.position, birthdayNotePosition.rotation);
        birthdayNote.isOnToilet = true;
    }

}
