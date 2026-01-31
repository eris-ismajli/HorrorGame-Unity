using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    public static SoundManager Instance { get; private set; }

    [SerializeField] private AudioClipRefsSO audioClipRefsSO;

    [SerializeField] private Transform player;
    [SerializeField] private Transform creepyGirlHead;

    [SerializeField] private Transform axe;

    private readonly string PORCELAIN_PICKUP = "PorcelainPickup";
    private readonly string METAL_PICKUP = "MetalPickup";
    private readonly string KEY_PICKUP = "KeyPickup";


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Door.OnDoorChanged += Door_OnDoorChanged;
        LightSwitchManager.OnLightSwitchChanged += LightSwitchManager_OnLightSwitchChanged;
        LightManager.OnLightFlicker += LightManager_OnLightFlicker;
        Door.OnDoorLocked += Door_OnDoorLocked;
        Pickable.OnPicked += Pickable_OnPicked;
        KeyAnimator.OnKeyTwist += KeyAnimator_OnKeyTwist;
        Door.OnFridgeOpen += Door_OnFridgeOpen;
        CircuitBreakerToggle.OnCircuitToggle += CircuitBreakerToggle_OnCircuitToggle;
        DrawerAnimator.OnDrawerChanged += DrawerAnimator_OnDrawerChanged;
        BreakPlank.OnPlankBreakSound += BreakPlank_OnPlankBreakSound;
        BreakPlank.OnPlankHit += BreakPlank_OnPlankHit1;
        PlayerInventory.Instance.OnEquip += Inventory_OnEquip;
        PlateFallAction.onPlateStartFall += Plate_OnPLateStartFall;
    }



    private void Plate_OnPLateStartFall(object sender, System.EventArgs e) {
        PlaySound(audioClipRefsSO.risingViolin, player.position, volume: 0.5f);
    }

    private void Inventory_OnEquip(object sender, System.EventArgs e) {
        PlaySound(audioClipRefsSO.equip, player.position, volume: 0.3f);
    }

    private void BreakPlank_OnPlankBreakSound(object sender, System.EventArgs e) {
        PlayRandomSound(audioClipRefsSO.planksBreaking, axe.position, volume: 0.5f);
    }
    private void BreakPlank_OnPlankHit1(object sender, System.EventArgs e) {
        PlaySound(audioClipRefsSO.axeChop, axe.position, volume: 0.5f);
    }


    private void DrawerAnimator_OnDrawerChanged(object sender, DrawerAnimator.OnDrawerChangedEventArgs e) {
        DrawerAnimator drawer = sender as DrawerAnimator;
        if (e.isOpen) {
            PlaySound(audioClipRefsSO.drawerOpen, drawer.transform.position);
        }
        else {
            PlaySound(audioClipRefsSO.drawerClose, drawer.transform.position);
        }
    }

    private void CircuitBreakerToggle_OnCircuitToggle(object sender, CircuitBreakerToggle.OnCircuitToggleEventArgs e) {
        CircuitBreakerToggle circuit = sender as CircuitBreakerToggle;
        if (e.isCircuitOn) {
            PlaySound(audioClipRefsSO.circuitOn, circuit.transform.position, volume: .6f);
        }
        else {
            PlaySound(audioClipRefsSO.circuitOff, circuit.transform.position, volume: .6f);
        }
    }

    private void Door_OnFridgeOpen(object sender, System.EventArgs e) {
        Door fridge = sender as Door;
        PlaySound(audioClipRefsSO.fridgeOpen, fridge.transform.position);
    }

    private void KeyAnimator_OnKeyTwist(object sender, System.EventArgs e) {
        KeyAnimator key = sender as KeyAnimator;
        PlaySound(audioClipRefsSO.keyTwist, key.transform.position);
    }


    private void Pickable_OnPicked(object sender, System.EventArgs e) {
        Pickable objectPicked = sender as Pickable;
        AudioClip defaultPickup = audioClipRefsSO.defaultPickup;

        AudioClip pickupSound;

        if (objectPicked.pickUpSound != null) {
            pickupSound = objectPicked.pickUpSound;
        }
        else {
            pickupSound = defaultPickup;
        }

        float volume = 0.45f;
        if (pickupSound.name == PORCELAIN_PICKUP) {
            volume = 0.23f;
        }
        else if (pickupSound.name == METAL_PICKUP) {
            volume = 0.1f;
        }
        else if (pickupSound.name == KEY_PICKUP) {
            volume = 1f;
        }
        PlaySound(pickupSound, objectPicked.transform.position, volume);
    }

    private void Door_OnDoorLocked(object sender, System.EventArgs e) {
        Door door = sender as Door;
        PlaySound(audioClipRefsSO.doorClosed, door.transform.position, volume: .53f);
    }

    private void LightManager_OnLightFlicker(object sender, System.EventArgs e) {
        LightManager light = sender as LightManager;
        PlayRandomSound(audioClipRefsSO.lightFlicker, light.transform.position, volume: .2f);
    }

    private void LightSwitchManager_OnLightSwitchChanged(object sender, LightSwitchManager.OnLightSwitchChangedEventArgs e) {
        LightSwitchManager lightSwitch = sender as LightSwitchManager;
        if (e.isOn) {
            PlaySound(audioClipRefsSO.lightSwitchOn, lightSwitch.transform.position, volume: 0.65f);
        }
        else {
            PlaySound(audioClipRefsSO.lightSwitchOff, lightSwitch.transform.position, volume: 0.65f);
        }
    }

    private void Door_OnDoorChanged(object sender, System.EventArgs e) {
        Door door = sender as Door;
        //PlayRandomSound(audioClipRefsSO.door, door.transform.position, volume: .4f);
        PlayUniqueAudio(door.transform.position, .4f, audioClipRefsSO.door);
    }

    private void PlayRandomSound(AudioClip[] audioClipArray, Vector3 position, float volume = 1f) {
        PlaySound(audioClipArray[Random.Range(0, audioClipArray.Length)], position, volume);
    }
    private void PlaySound(AudioClip audioClip, Vector3 position, float volume = 1f) {
        if (audioClip.name == "SFX_Footstep1") {
            volume = 0.6f;
        }

        AudioSource.PlayClipAtPoint(audioClip, position, volume);
    }

    private int lastSoundIndex = -1;

    private void PlayUniqueAudio(Vector3 position, float volume, AudioClip[] audios) {

        int newIndex;

        do {
            newIndex = Random.Range(0, audios.Length);
        } while (newIndex == lastSoundIndex);

        lastSoundIndex = newIndex;
        AudioClip clip = audios[newIndex];

        if (clip.name == "SFX_Door1" || clip.name == "SFX_Door3") {
            PlaySound(clip, position, volume: .2f);
        }
        else {
            PlaySound(clip, position, volume);
        }
    }

    private int lastFootstepIndex = -1;
    public void PlayFootstepSound(Vector3 position, float volume) {
        int newIndex;
        do {
            newIndex = Random.Range(0, audioClipRefsSO.footsteps.Length);
        } while (newIndex == lastFootstepIndex);

        lastFootstepIndex = newIndex;
        AudioClip clip = audioClipRefsSO.footsteps[newIndex];

        PlaySound(clip, position, volume);
    }


    public void PlayLightbulbBreakSound1(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.lightbulbBreak1, position, volume);
    }

    public void PlayLightbulbBreakSound2(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.lightbulbBreak2, position, volume);
    }

    public void PlayPlateSlidingOne(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.plateSliding, position, volume);
    }

    public void PlayPlateSlidingTwo(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.plateSlidingSecond, position, volume);
    }

    public void PlayPlateSlidingThree(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.plateSlidingThird, position, volume);
    }

    public void PlayPlateBreakingSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.plateBreaking, position, volume);
    }

    public void PlayWhisperTrailSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.whisperTrail, position, volume);
    }

    public void PlayMugSlideOne(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.mugSlide1, position, volume);
    }

    public void PlayMugSlideTwo(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.mugSlide2, position, volume);
    }

    public void PlayCrawlingSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.crawl, position, volume);
    }

    public void PlayDoorCloseSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.doorClose, position, volume);
    }

    public void PlayDoorOpenSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.doorOpen, position, volume);
    }

    public void PlayPlayerKnockingSound(Vector3 position, float volume) {
        PlaySound(audioClipRefsSO.playerDoorKnock, position, volume);
    }

}
