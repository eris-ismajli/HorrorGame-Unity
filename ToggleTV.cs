using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

public class ToggleTV : IsHoverable {

    public static ToggleTV Instance { get; private set; }

    [SerializeField] private EquipableObjectSO tvRemoteEquipableSO;
    [SerializeField] private AudioSource tvStaticNoise;
    [SerializeField] private VideoPlayer tvStaticVideo;
    [SerializeField] private GameObject tvLight;

    [SerializeField] private Material onMaterial;
    [SerializeField] private Material offMaterial;

    [SerializeField] private GameObject tvScreenMesh;

    private MeshRenderer tvScreenRenderer;

    public bool isTVon;


    void Awake() {
        Instance = this;
        tvScreenRenderer = tvScreenMesh.GetComponent<MeshRenderer>();
    }

    void Start() {
        isInteractable = false;

        Pickable.OnTvRemoteEquipped += TvRemote_OnEquipped;
    }

    private void TvRemote_OnEquipped(object sender, EventArgs e) {
        isInteractable = true;
    }

    private void ToggleMaterial(bool on) {
        Material targetMat = on ? onMaterial : offMaterial;
        if (tvScreenRenderer != null) tvScreenRenderer.material = targetMat;
    }

    public void ToggleTVScreen() {
        isTVon = !isTVon;
        ToggleMaterial(isTVon);
        if (isTVon) {
            tvStaticNoise.Play();
            tvStaticVideo.Play();
            tvLight.SetActive(true);
        }
        else {
            tvStaticNoise.Stop();
            tvStaticVideo.Stop();
            tvLight.SetActive(false);
        }

        if (AnimationFunctions.Instance.isGirlInCorner) {
            StartCoroutine(WaitBeforeScreamJumpscare());
        }
    }

    private IEnumerator WaitBeforeScreamJumpscare() {
        yield return new WaitForSeconds(2f);

        if (!GirlScreamAction.Instance.triggered) {
            GirlScreamAction.Instance.GirlScreamJumpscare();
        }
    }

    protected override void HandleMouseDown() {
        if (PlayerInventory.Instance.HasEquipableObject(tvRemoteEquipableSO)) {
            ToggleTVScreen();
        }
    }

}
