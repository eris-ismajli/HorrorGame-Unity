using System;
using UnityEngine;

public class GirlScreamAction : MonoBehaviour {

    public static GirlScreamAction Instance { get; private set; }
    public static event EventHandler OnGirlScream;

    public bool triggered = false;


    void Awake() {
        Instance = this;
    }

    void OnTriggerEnter(Collider other) {
        if (triggered) return;

        if (other.CompareTag("Player")) {
            GirlScreamJumpscare();
        }
    }

    public void GirlScreamJumpscare() {
        triggered = true;
        OnGirlScream?.Invoke(this, EventArgs.Empty);
        AnimationFunctions.Instance.EndGirlAnimation();
        Destroy(gameObject);
    }
}
