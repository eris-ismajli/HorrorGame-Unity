using UnityEngine;

public class LightFlicker : MonoBehaviour {


    [SerializeField] private LightManager lightBulb;
    [SerializeField] private float minInterval = 0.03f;
    [SerializeField] private float maxInterval = 0.15f;
    [SerializeField] private float flickerChance = 0.5f;

    private bool _isFlickering = false;
    private float _nextToggleTime;

    private void Update() {
        if (!_isFlickering || !lightBulb.DoesFlicker()) return;

        if (Time.time >= _nextToggleTime) {
            if (lightBulb != null) {
                bool on = lightBulb.IsThisLightOn()
                ? (Random.value > flickerChance)
                : true;

                lightBulb.FlickerEffect(on);
                ScheduleNextToggle();
            }
        }
    }

    public void StartFlicker() {
        if (_isFlickering) return;

        _isFlickering = true;
        // Ensure we begin in a known state
        ScheduleNextToggle();
    }

    private void ScheduleNextToggle() {
        _nextToggleTime = Time.time + Random.Range(minInterval, maxInterval);
    }
}
