using UnityEngine;

public class MugSlideSoundEffect : MonoBehaviour {

    public void SlideOne() {
        SoundManager.Instance.PlayMugSlideOne(transform.position, .75f);
    }

    public void SlideTwo() {
        SoundManager.Instance.PlayMugSlideTwo(transform.position, .75f);
    }
}
