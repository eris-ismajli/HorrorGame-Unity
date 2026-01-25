using UnityEngine;

public class PlateAnimationEvents : MonoBehaviour {
    public PlateFallAction plateFallAction;

    public void ReleasePlate() {
        plateFallAction.ReleasePlate();
        SoundManager.Instance.PlayPlateBreakingSound(transform.position, 1f);
    }

    public void FirstSlide() {
        SoundManager.Instance.PlayPlateSlidingOne(transform.position, .22f);
    }

    public void SecondSlide() {
        SoundManager.Instance.PlayPlateSlidingTwo(transform.position, .27f);
    }

    public void ThirdSlide() {
        SoundManager.Instance.PlayPlateSlidingThree(transform.position, 1f);
    }

}
