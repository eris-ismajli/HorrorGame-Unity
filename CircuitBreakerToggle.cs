using System;
using UnityEngine;

public class CircuitBreakerToggle : IsHoverable {

    public static event EventHandler<OnCircuitToggleEventArgs> OnCircuitToggle;

    public class OnCircuitToggleEventArgs : EventArgs {
        public bool isCircuitOn;
    }

    [SerializeField] private Transform keyOnPos;
    [SerializeField] private Transform keyOffPos;
    [SerializeField] private Transform circuitKey;
    [SerializeField] private LightManager correspondingLight;
    [SerializeField] private LightSwitchManager correspondingLightSwitch;
    [SerializeField] private bool isCircuitOn;

    private void Start() {
        SetupCircuit();
        //correspondingLight.ToggleLight(isCircuitOn);
    }
    protected override void HandleMouseDown() {
        ToggleCircuit();
    }

    public void ToggleCircuit() {
        isCircuitOn = !isCircuitOn;

        OnCircuitToggle?.Invoke(this, new OnCircuitToggleEventArgs {
            isCircuitOn = isCircuitOn,
        });

        SetupCircuit();

    }

    private void SetupCircuit() {
        // pick the target transform
        Transform target = isCircuitOn ? keyOnPos : keyOffPos;

        // world‑space:
        circuitKey.SetPositionAndRotation(target.position, target.rotation);

        if (correspondingLight.isBroken) return;

        if (correspondingLightSwitch.IsSwitchOn()) {
            correspondingLight.ToggleLightFromCircuit(isCircuitOn);
        }
    }

    public bool IsOn() {
        return isCircuitOn;
    }

}
