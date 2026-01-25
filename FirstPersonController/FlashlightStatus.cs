using UnityEngine;

public class FlashlightStatus : MonoBehaviour {

    public static FlashlightStatus Instance { get; private set; }

    [SerializeField] private EquipableObjectSO flashlightSO;

    [SerializeField] Light flashLight;

    private bool isFlashLightOn = false;
    private bool canBeToggled = false;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        flashLight.enabled = isFlashLightOn;
    }

    private void Update() {
        if (!canBeToggled) return;
        if (Input.GetKeyDown(KeyCode.F)) {
            ToggleFlashlight();
        }
    }

    public void CanBeToggled(bool enable) {
        canBeToggled = enable;
    }

    public void ToggleFlashlight() {
        isFlashLightOn = !isFlashLightOn;
        flashLight.enabled = isFlashLightOn;
    }


    public bool IsFlashlighOn() {
        return isFlashLightOn;
    }
}
