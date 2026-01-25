using System;
using UnityEngine;

public class LightSwitchManager : IsHoverable {


    public static event EventHandler<OnLightSwitchChangedEventArgs> OnLightSwitchChanged;

    public class OnLightSwitchChangedEventArgs : EventArgs {
        public bool isOn;
    }

    [SerializeField] private LightManager lightBulb;
    [SerializeField] private GameObject lightSwitch;
    [SerializeField] private Transform switchOn;
    [SerializeField] private Transform switchOff;


    private bool isLightOn = false;

    protected override void HandleMouseDown() {
        ToggleLightSwitch();
    }

    public void ToggleLightSwitch() {
        isLightOn = !isLightOn;

        OnLightSwitchChanged?.Invoke(this, new OnLightSwitchChangedEventArgs {
            isOn = isLightOn
        });

        if (lightSwitch != null) {
            lightSwitch.transform.rotation = isLightOn ? switchOn.rotation : switchOff.rotation;
        }

        if (lightBulb != null) {
            lightBulb.ToggleLight(isLightOn);
            if (isLightOn && lightBulb.DoesFlicker()) {
                lightBulb.StartFlicker();
            }
        }

    }

    public bool IsSwitchOn() {
        return isLightOn;
    }

}
