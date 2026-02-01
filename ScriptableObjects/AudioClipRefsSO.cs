using UnityEngine;

[CreateAssetMenu()]
public class AudioClipRefsSO : ScriptableObject {

    public static AudioClipRefsSO Instance {get; private set;}

    void Awake() {
       Instance = this; 
    }

    public AudioClip[] door;
    public AudioClip doorClosed;
    public AudioClip doorClose;
    public AudioClip doorOpen;
    public AudioClip playerDoorKnock;

    public AudioClip lightSwitchOn;
    public AudioClip lightSwitchOff;

    public AudioClip[] lightFlicker;

    public AudioClip[] footsteps;

    [Header("====== Pickups =======")]
    public AudioClip defaultPickup;
    public AudioClip porcelainPickup;
    public AudioClip paperPickup;
    public AudioClip glassPickup;
    public AudioClip keyPickup;
    public AudioClip canPickup;
    [Header("======================")]

    public AudioClip keyTwist;


    public AudioClip fridgeOpen;

    public AudioClip circuitOn;
    public AudioClip circuitOff;

    public AudioClip drawerOpen;
    public AudioClip drawerClose;

    public AudioClip doorKnock;


    [Header("====== Horror =======")]
    public AudioClip risingViolin;
    public AudioClip scaryViolins;
    public AudioClip demonicScream;
    [Header("=====================")]

    public AudioClip whisper;

    public AudioClip lightbulbBreak1;
    public AudioClip lightbulbBreak2;

    public AudioClip plateSliding;
    public AudioClip plateSlidingSecond;
    public AudioClip plateSlidingThird;
    public AudioClip plateBreaking;

    public AudioClip axeChop;
    public AudioClip[] planksBreaking;

    public AudioClip whisperTrail;

    public AudioClip mugSlide1;
    public AudioClip mugSlide2;

    public AudioClip crawl;

    public AudioClip equip;
}
