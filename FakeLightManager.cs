using UnityEngine;

public class FakeLightManager : MonoBehaviour {
    [SerializeField] private LightManager hallLight;
    [SerializeField] private LightManager kitchenLight;
    [SerializeField] private LightManager livingRoomLight;

    [SerializeField] private Light[] hallFakeLights;
    [SerializeField] private Light[] kitchenFakeLights;
    [SerializeField] private Light[] livingRoomFakeLights;

    private void OnEnable() {
        hallLight.OnLightToggled += OnAnyLightToggled;
        kitchenLight.OnLightToggled += OnAnyLightToggled;
        livingRoomLight.OnLightToggled += OnAnyLightToggled;
        Apply(); // initialize correct state on load
    }

    private void OnDisable() {
        hallLight.OnLightToggled -= OnAnyLightToggled;
        kitchenLight.OnLightToggled -= OnAnyLightToggled;
        livingRoomLight.OnLightToggled -= OnAnyLightToggled;
    }

    private void OnAnyLightToggled(object sender, System.EventArgs e) => Apply();

    private void Apply() {
        bool hallOn = hallLight.IsThisLightOn();
        bool kitchenOn = kitchenLight.IsThisLightOn();
        bool livingOn = livingRoomLight.IsThisLightOn();

        ToggleFakeLights(hallFakeLights, !kitchenOn);
        ToggleFakeLights(kitchenFakeLights, !hallOn);
        ToggleFakeLights(livingRoomFakeLights, !kitchenOn);
    }

    private void ToggleFakeLights(Light[] fakeLightsArray, bool enabled) {
        if (fakeLightsArray == null) return;
        foreach (var l in fakeLightsArray) {
            if (!l) continue;
            // choose ONE of these based on your setup:
            // l.enabled = enabled;                     // if Light is the only thing to toggle
            l.gameObject.SetActive(enabled);            // if the whole object should disappear
        }
    }
}
