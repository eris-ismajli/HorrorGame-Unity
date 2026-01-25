using System;
using UnityEngine;

public class PadlockKeyAnim : MonoBehaviour {

    public static event EventHandler OnLockerKeyPullout;

    private MeshRenderer keyRenderer;

    private void Awake() {
        keyRenderer = GetComponent<MeshRenderer>();
    }

    public void OnLockerKeyPullOut() {
        OnLockerKeyPullout?.Invoke(this, EventArgs.Empty);
        keyRenderer.enabled = false;
    }
}
