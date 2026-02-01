using System;
using System.Collections;
using UnityEngine;

public class DelayedEventOnPickupManager : MonoBehaviour {

    // Pickup name constants
    private const string STORAGE_KEY = "Storage Key";
    private const string CROWBAR = "Crowbar";

    // Function specific variables
    [SerializeField] private LightSwitchManager hallLightSwitch;
    [SerializeField] private LightSwitchManager kitchenLightSwitch;

    public static event EventHandler OnMugSlide;

    void Start() {
        Pickable.OnPicked += Pickable_OnPicked;
    }

    private void OnDestroy() {
        Pickable.OnPicked -= Pickable_OnPicked;
    }


    private IEnumerator WaitBeforeExecuting(float waitTime, Action eventAction) {
        yield return new WaitForSeconds(waitTime);
        eventAction?.Invoke();
    }

    private void KitchenHallLightsOff() {
        if (hallLightSwitch.IsSwitchOn() && kitchenLightSwitch.IsSwitchOn()) {
            hallLightSwitch.ToggleLightSwitch(silentSwitch: true);
            kitchenLightSwitch.ToggleLightSwitch(silentSwitch: true);
        }
    }

    private void HallLightOffMugSlide() {
        if (hallLightSwitch.IsSwitchOn()) {
            hallLightSwitch.ToggleLightSwitch();
            OnMugSlide?.Invoke(this, EventArgs.Empty);
        }
    }
    private void ConditionalExecutionOnPickup(string objectPickedName) {
        if (objectPickedName == "") {
            return;
        }

        switch (objectPickedName) {
            case STORAGE_KEY:
                StartCoroutine(WaitBeforeExecuting(.5f, KitchenHallLightsOff));
                break;
            case CROWBAR:
                StartCoroutine(WaitBeforeExecuting(4f, HallLightOffMugSlide));
                break;
            default:
                Debug.Log("case for " + objectPickedName + " not handled");
                break;
        }
    }

    private void Pickable_OnPicked(object sender, Pickable.OnPickedEventArgs e) {
        ConditionalExecutionOnPickup(e.objectPickedName);
    }

}
