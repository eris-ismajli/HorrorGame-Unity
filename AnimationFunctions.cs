using System.Collections;
using UnityEngine;

public class AnimationFunctions : MonoBehaviour {
    [SerializeField] private LightManager livingRoomLight;
    [SerializeField] private LightSwitchManager kitchenLightSwitch;
    [SerializeField] private LightManager hallLight;
    [SerializeField] private LightManager entranceLight;
    [SerializeField] private GameObject bloodyFingerprints;
    [SerializeField] private GameObject stopHallFlickerTrigger;
    [SerializeField] private Transform passiveGirlPos;

    [SerializeField] private float minDelay = 0.03f;
    [SerializeField] private float maxDelay = 0.12f;

    private bool canFlicker = true;
    private Coroutine flickerRoutine;
    private bool lightOn;
    public bool isFlickering = false;

    private bool turnedKitchenLightOff = false;

    private Animator girlAnim;

    private void Awake() => girlAnim = GetComponent<Animator>();

    public void EndCrawl() {
        StartCoroutine(SnapAndIdle());
        livingRoomLight.BreakLight();
        SoundManager.Instance.PlayLightbulbBreakSound2(Camera.main.transform.position, 0.8f);
    }

    public void EndGirlAnimation() {
        StartCoroutine(SnapAndIdle());
    }

    public void TurnOffKitchenLight() {
        kitchenLightSwitch.ToggleLightSwitch();
        turnedKitchenLightOff = true;
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
        if (!turnedKitchenLightOff) {
            kitchenLightSwitch.ToggleLightSwitch();
        }
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
