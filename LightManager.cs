using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class LightManager : LightFlicker {

    public static event EventHandler OnLightFlicker;
    public event EventHandler OnLightToggled;

    [SerializeField] private Light[] lightsArray;
    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;
    [SerializeField] private CircuitBreakerToggle correspondingCircuit;
    [SerializeField] private LightSwitchManager correspondingLightSwitch;
    [SerializeField] private bool doesFlicker = false;

    [SerializeField] private GameObject cylinder;

    [Range(0f, 1f)]
    [SerializeField] private float dimFactor = 0.35f;


    private float[] originalIntensities;
    [SerializeField] private bool isThisLightOn = false;
    private MeshRenderer bulbRenderer;

    private bool isBusy = false;
    public bool isBroken = false;

    private void Awake() {

        bulbRenderer = gameObject.GetComponent<MeshRenderer>();

        originalIntensities = new float[lightsArray.Length];

        for (int i = 0; i < lightsArray.Length; i++) {
            originalIntensities[i] = lightsArray[i].intensity;
        }
    }

    private void Start() {
        ToggleLight(isThisLightOn);
    }

    public void ToggleLight(bool isLightOn) {
        if (isBusy || isBroken) return;

        if (correspondingCircuit != null && !correspondingCircuit.IsOn()) {
            if (correspondingLightSwitch != null && correspondingLightSwitch.IsSwitchOn()) {
                StartCoroutine(ShowLightError("No power to this light. Check the circuit breakers in the entrance."));
            }
            ApplyState(false);
            return;
        }
        ApplyState(isLightOn);
    }

    public void BreakLight() {
        isBroken = true;
        ApplyState(false);
        if (cylinder != null) {
            cylinder.transform.SetParent(null, true);
            Destroy(gameObject);
        }
    }

    public void ToggleLightFromCircuit(bool isLightOn) {
        if (correspondingLightSwitch != null && correspondingLightSwitch.IsSwitchOn()) {
            ApplyState(isLightOn);
        }
    }

    private IEnumerator ShowLightError(string msg) {
        isBusy = true;
        FeedbackUIManager.Instance.ShowFeedback(msg);
        yield return new WaitForSeconds(3f);
        isBusy = false;
    }

    private void ApplyState(bool on) {
        isThisLightOn = on;
        OnLightToggled?.Invoke(this, EventArgs.Empty);
        Material targetMat = on ? onMaterial : offMaterial;
        foreach (var light in lightsArray) light.enabled = on;
        if (bulbRenderer != null) bulbRenderer.material = targetMat;
    }

    public void FlickerEffect(bool isOn) {
        for (int i = 0; i < lightsArray.Length; i++) {
            float orig = originalIntensities[i];
            lightsArray[i].intensity = isOn ? orig : orig * dimFactor;
        }
        if (!isOn) {
            OnLightFlicker?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool IsThisLightOn() {
        return isThisLightOn;
    }

    public bool DoesFlicker() {
        return doesFlicker;
    }
}
