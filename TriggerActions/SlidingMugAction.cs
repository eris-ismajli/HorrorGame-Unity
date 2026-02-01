using UnityEngine;

public class SlidingMugAction : MonoBehaviour {

    [SerializeField] private GameObject mug;
    [SerializeField] private StopMugSlide StopMugSlideTrigger;

    private Animator mugAnimator;

    private void Awake() {
        mugAnimator = mug.GetComponent<Animator>();
    }

    private void Start() {
        DelayedEventOnPickupManager.OnMugSlide += Crowbar_OnMugSlide;
        StopMugSlideTrigger.OnStopMugSlide += StopMugSlideTrigger_OnStopMugSlide;
    }

    private void Crowbar_OnMugSlide(object sender, System.EventArgs e) {
        mugAnimator.SetTrigger("SlideMug");
        StopMugSlideTrigger.gameObject.SetActive(true);
    }

    private void StopMugSlideTrigger_OnStopMugSlide(object sender, System.EventArgs e) {
        mugAnimator.SetTrigger("StopSliding");
        DelayedEventOnPickupManager.OnMugSlide -= Crowbar_OnMugSlide;
        Destroy(gameObject);
    }

}
