using System.Collections;
using UnityEngine;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameObject mainFog;
    [SerializeField] private GameObject stairwellFog;
    [SerializeField] private GameObject stairWell;
    [SerializeField] private Door entranceDoor;

    [SerializeField] private AudioSource horrorAtmosphere;

    public enum CurrentGameState {
        Beginning,
        Middle,
        Ending
    }

    public CurrentGameState currentGameState;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() {
        SetGameState(CurrentGameState.Beginning, force: true);
        AllLightsOffAction.OnAllLightsOff += LightsOff_OnAllLightsOff;
    }

    private void LightsOff_OnAllLightsOff(object sender, System.EventArgs e) {
        StartCoroutine(PrepareAndPlayAmbience());
    }

    public void SetGameState(CurrentGameState newState, bool force = false) {
        if (!force && currentGameState == newState)
            return;

        currentGameState = newState;

        switch (currentGameState) {
            case CurrentGameState.Beginning:
                stairwellFog.SetActive(true);
                mainFog.SetActive(false);
                break;

            case CurrentGameState.Middle:
                stairwellFog.SetActive(false);
                mainFog.SetActive(true);
                break;

            case CurrentGameState.Ending:
                stairwellFog.SetActive(true);
                entranceDoor.UnlockFromTriggerKeepClosed();
                stairWell.SetActive(true);
                // mainFog.SetActive(false);
                break;
        }
    }

    private IEnumerator PrepareAndPlayAmbience() {
        var clip = horrorAtmosphere.clip;
        if (clip == null) yield break;

        // Kick off async load (if not already loaded)
        if (!clip.preloadAudioData && clip.loadState != AudioDataLoadState.Loaded) {
            clip.LoadAudioData();
            while (clip.loadState == AudioDataLoadState.Loading) {
                yield return null; // wait a few frames without blocking
            }
        }

        // Schedule start slightly in the future to avoid sync hitches
        double startTime = AudioSettings.dspTime + 0.05;
        horrorAtmosphere.PlayScheduled(startTime);
    }

}
