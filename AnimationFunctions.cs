using System.Collections;
using UnityEngine;

public class AnimationFunctions : MonoBehaviour {

    public static AnimationFunctions Instance { get; private set; }

    [SerializeField] private LightManager livingRoomLight;
    [SerializeField] private LightSwitchManager kitchenLightSwitch;
    [SerializeField] private LightManager hallLight;
    [SerializeField] private LightManager entranceLight;
    [SerializeField] private GameObject bloodyFingerprints;
    [SerializeField] private GameObject stopHallFlickerTrigger;
    [SerializeField] private Transform passiveGirlPos;

    [SerializeField] private float minDelay = 0.03f;
    [SerializeField] private float maxDelay = 0.12f;

    [SerializeField] private EquipableObjectSO flashlightSO;

    [SerializeField] private GameObject girlScreamTrigger;

    private bool canFlicker = true;
    private Coroutine flickerRoutine;
    private bool lightOn;
    public bool isFlickering = false;

    private Animator girlAnim;

    public bool isGirlInCorner = false;

    private float playerSpeed;

    private void Awake() {
        Instance = this;
        girlAnim = GetComponent<Animator>();
    }

    private void Start() {
        playerSpeed = FirstPersonController.Instance.speed;
    }

    public void EndCrawl() {
        StartCoroutine(SnapAndIdle());
        livingRoomLight.BreakLight();
        SoundManager.Instance.PlayLightbulbBreakSound2(Camera.main.transform.position, 0.8f);
        StartCoroutine(WaitBeforeTVOn());
    }

    private IEnumerator WaitBeforeTVOn() {
        yield return new WaitForSeconds(2f);
        if (!ToggleTV.Instance.isTVon) {
            ToggleTV.Instance.ToggleTVScreen();
            if (!PlayerInventory.Instance.HasEquipableObject(flashlightSO)) {
                girlAnim.SetTrigger("LivingRoomCorner");
                girlScreamTrigger.SetActive(true);
                isGirlInCorner = true;
            }
        }
        yield return new WaitForSeconds(0.5f);
        FirstPersonController.Instance.canRun = true;
        FirstPersonController.Instance.speed = playerSpeed;
    }

    public void EndGirlAnimation() {
        StartCoroutine(SnapAndIdle());
    }

    public void TurnOffKitchenLight() {
        kitchenLightSwitch.ToggleLightSwitch();
        bloodyFingerprints.SetActive(true);
    }


    public void FlickerHallLight() {
        if (!canFlicker || flickerRoutine != null) return;
        isFlickering = true;
        canFlicker = true;
        flickerRoutine = StartCoroutine(FlickerLoop());
    }

    IEnumerator FlickerLoop() {
        while (canFlicker) {
            lightOn = !lightOn;
            hallLight.ToggleLight(lightOn);

            yield return new WaitForSeconds(Random.Range(minDelay, maxDelay));
        }
    }

    public void StopFlicker() {
        canFlicker = false;
        isFlickering = false;
        if (flickerRoutine != null) {
            StopCoroutine(flickerRoutine);
            flickerRoutine = null;
        }
        hallLight.ToggleLight(false);
        entranceLight.ToggleLight(false);

    }

    public void TurnLightsBackOn() {
        hallLight.ToggleLight(true);
        entranceLight.ToggleLight(true);
    }

    private IEnumerator SnapAndIdle() {
        // Stop animator from writing transforms for a moment
        girlAnim.enabled = false;

        // Hard snap
        transform.SetPositionAndRotation(passiveGirlPos.position, passiveGirlPos.rotation);

        // Wait one frame so nothing else overwrites it
        yield return null;

        // Re-enable and force Idle immediately (no blending)
        girlAnim.enabled = true;
        girlAnim.Play("Idle", 0, 0f);
        girlAnim.Update(0f);

    }
}
