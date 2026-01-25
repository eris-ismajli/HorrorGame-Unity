using System.Collections;
using UnityEngine;

public class StopHallFlicker : MonoBehaviour {
    [SerializeField] private AnimationFunctions girlAnimationFunctions;

    private bool triggered = false;

    private void OnTriggerEnter(Collider other) {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(GirlVanish());
    }

    private IEnumerator GirlVanish() {
        if (girlAnimationFunctions != null) {
            girlAnimationFunctions.StopFlicker();

            yield return new WaitForSeconds(1.5f);

            SafeCodeManager.Instance.canStareAtPlayer = false;
            girlAnimationFunctions.EndGirlAnimation();

            yield return null;

            girlAnimationFunctions.TurnLightsBackOn();
        }

        Destroy(gameObject);
    }
}
