using UnityEngine;
using System;
using System.Collections;

public class StormVolumeManager : MonoBehaviour {
    public static StormVolumeManager Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private AudioSource stormAudio;

    [Header("Zone-driven settings")]
    [SerializeField] private float targetVolume = 1f;
    [SerializeField] private float fadeSpeed = 1.5f; // still used for zone logic

    [Header("Fade-out curve (0..1 time → 0..1 blend)")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private int activeZones = 0;
    private bool forceFadeOut = false;
    private Coroutine fadeRoutine;

    [SerializeField] private GameObject stormAmbienceManager;


    private void Awake() {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        if (stormAudio != null && !stormAudio.isPlaying) {
            stormAudio.Play(); // ensure source is alive to fade
        }
    }

    public void EnterZone() { activeZones++; }
    public void ExitZone() { activeZones = Mathf.Max(0, activeZones - 1); }

    private void Update() {
        // While a forced fade-out is running, ignore zone logic.
        if (forceFadeOut) return;

        float desiredVolume = activeZones > 0 ? targetVolume : 0.06f;
        if (stormAudio != null) {
            stormAudio.volume = Mathf.MoveTowards(stormAudio.volume, desiredVolume, Time.deltaTime * fadeSpeed);
        }
    }

    /// <summary>
    /// Fades the storm audio all the way to 0 over 'duration' seconds.
    /// Optionally stops the AudioSource at the end to save CPU.
    /// </summary>
    public void FadeOutCompletely(float duration = 8f, bool stopAtEnd = true) {
        if (stormAudio == null) return;

        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeOutRoutine(duration, stopAtEnd));
    }

    /// <summary>
    /// Cancels an in-progress forced fade and returns to zone-driven control.
    /// </summary>
    public void CancelFadeOut() {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = null;
        forceFadeOut = false;
    }

    private IEnumerator FadeOutRoutine(float duration, bool stopAtEnd) {
        forceFadeOut = true;

        // Ensure source is playing so volume changes apply consistently.
        if (!stormAudio.isPlaying) stormAudio.Play();

        float startVol = stormAudio.volume;
        float t = 0f;
        duration = Mathf.Max(0.001f, duration);

        while (t < 1f) {
            t += Time.deltaTime / duration;
            float k = fadeCurve != null ? Mathf.Clamp01(fadeCurve.Evaluate(Mathf.Clamp01(t))) : Mathf.Clamp01(t);
            stormAudio.volume = Mathf.Lerp(startVol, 0f, k);
            yield return null;
        }

        stormAudio.volume = 0f;

        if (stopAtEnd) stormAudio.Stop();

        forceFadeOut = false;
        fadeRoutine = null;
        Destroy(stormAmbienceManager);
    }
}
