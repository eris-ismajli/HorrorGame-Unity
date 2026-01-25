using UnityEngine;

public class PadlockAnim : MonoBehaviour {
    public void OnPadlockOpen() {
        Destroy(gameObject);
    }
}
