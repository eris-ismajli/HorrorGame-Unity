using UnityEngine;

public class DistanceBasedLightFlicker : MonoBehaviour {

    public static DistanceBasedLightFlicker Instance {get; private set;}

    [SerializeField] private LightManager hallLight;
    [SerializeField] private LightSwitchManager hallLightSwitch;
    [SerializeField] private Transform player;

    [Header("Distance gates (meters)")]
    [SerializeField] private float startFlickerDistance = 8f;   // outside this: steady/slow
    [SerializeField] private float fullSpeedDistance = 1.2f;  // inside this: at minInterval

    [Header("Intervals (seconds)")]
    [SerializeField] private float maxInterval = 2.0f;   // far
    [SerializeField] private float minInterval = 0.12f;  // near (>= ~0.1 to avoid buzz)

    [Header("Smoothing (optional)")]
    [SerializeField] private float distanceSmooth = 0.15f; // small low-pass; set 0 for none

    private float timer;
    private float smoothD;    // smoothed distance
    private float dVel;       // SmoothDamp velocity

    private bool canFlicker = true;

    public bool isGirlVisible = false;

    void Awake() {
       Instance = this; 
    }

    void Start() {
        smoothD = startFlickerDistance; // init “far”
        timer = 0f;
    }

    void Update() {
        if (!canFlicker || isGirlVisible) return;
        // 1) Measure (optionally smooth a bit to prevent tiny jitter)
        float rawD = Vector3.Distance(hallLightSwitch.transform.position, player.position);
        smoothD = (distanceSmooth > 0f)
            ? Mathf.SmoothDamp(smoothD, rawD, ref dVel, distanceSmooth)
            : rawD;

        // 2) Map distance → 0..1 where 0 = far (>= start), 1 = near (<= fullSpeed)
        float t = Mathf.InverseLerp(startFlickerDistance, fullSpeedDistance, smoothD);

        // 3) Ease curve so it accelerates *late* (no sudden jump when barely closer)
        // SmoothStep: t*t*(3-2t)
        float eased = t * t * (3f - 2f * t);

        // 4) Final interval
        float interval = Mathf.Lerp(maxInterval, minInterval, eased);

        // 5) Simple scheduler (max 1 toggle per frame; no while-loop bursts)
        timer += Time.deltaTime;
        if (timer >= interval) {
            hallLightSwitch.ToggleLightSwitch();
            timer = 0f;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            SoundManager.Instance.PlayLightbulbBreakSound1(hallLight.transform.position, 1f);
            hallLight.BreakLight();
            canFlicker = false;
            if (FlashlightStatus.Instance.IsFlashlighOn()) {
                FlashlightStatus.Instance.ToggleFlashlight();
            }
            Destroy(gameObject);
        }
    }
}
