using UnityEngine;

public class SlidingMugAction : MonoBehaviour {

    [SerializeField] private GameObject mug;
    [SerializeField] private StopMugSlide StopMugSlideTrigger;
    [SerializeField] private Pickable crowbar;

    private Animator mugAnimator;

    private void Awake() {
        mugAnimator = mug.GetComponent<Animator>();
    }

    private void Start() {
        crowbar.OnMugSlide += Crowbar_OnMugSlide;
        StopMugSlideTrigger.OnStopMugSlide += StopMugSlideTrigger_OnStopMugSlide;
    }

    private void Crowbar_OnMugSlide(object sender, System.EventArgs e) {
        mugAnimator.SetTrigger("SlideMug");
        StopMugSlideTrigger.gameObject.SetActive(true);
    }

    private void StopMugSlideTrigger_OnStopMugSlide(object sender, System.EventArgs e) {
        mugAnimator.SetTrigger("StopSliding");
        crowbar.OnMugSlide -= Crowbar_OnMugSlide;
        Destroy(gameObject);
    }

}
