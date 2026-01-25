using UnityEngine;

public class GameStateManager : MonoBehaviour {
    public static GameStateManager Instance { get; private set; }

    [SerializeField] private GameObject mainFog;
    [SerializeField] private GameObject stairwellFog;
    [SerializeField] private GameObject stairWell;
    [SerializeField] private Door entranceDoor;

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
    }

    public void SetGameState(CurrentGameState newState, bool force = false) {
        if (!force && currentGameState == newState)
            return;

        currentGameState = newState;

        switch (currentGameState) {
            case CurrentGameState.Beginning:
                Debug.Log("beginning");
                stairwellFog.SetActive(true);
                mainFog.SetActive(false);
                break;

            case CurrentGameState.Middle:
                Debug.Log("middle");
                stairwellFog.SetActive(false);
                mainFog.SetActive(true);
                break;

            case CurrentGameState.Ending:
                Debug.Log("ending");
                stairwellFog.SetActive(true);
                entranceDoor.UnlockFromTriggerKeepClosed();
                stairWell.SetActive(true);
                // mainFog.SetActive(false);
                break;
        }
    }

}
