using UnityEngine;

public class LivingRoomLightOff : MonoBehaviour {

    [SerializeField] private LightManager livingRoomLight;
    [SerializeField] private LightSwitchManager livingRoomLightSwitch;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (livingRoomLight.IsThisLightOn()) {
                livingRoomLightSwitch.ToggleLightSwitch();
                Destroy(gameObject);
            }
        }
    }
}
