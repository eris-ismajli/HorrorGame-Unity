using System;
using System.Collections;
using UnityEngine;

public class OnPasswordFound : MonoBehaviour {

    //[SerializeField] private Door storageDoor;
    //[SerializeField] private LightManager storageLight;
    [SerializeField] private GameObject distanceBasedLightFlicker;

    [SerializeField] private Pickable[] pickables;

    private int currentPickable = 0;

    private void Start() {
        PlayerPickupController.Instance.OnObjectEquipped += PlayerPickupController_OnObjectEquipped;
        CameraFocusController.OnZoomOutAfterOpening += CameraFocusController_OnZoomOutAfterOpening;
    }


    private void PlayerPickupController_OnObjectEquipped(object sender, PlayerPickupController.OnObjectEquippedEventArgs e) {
        foreach (Pickable pickable in pickables) {
            if (e.objectEquipped == pickable) {
                currentPickable++;
                StartCoroutine(WaitBeforeNextPick());
            }
        }
    }
    private void CameraFocusController_OnZoomOutAfterOpening(object sender, EventArgs e) {
        PlayerPickupController.Instance.TryPick(pickables[currentPickable]);
        distanceBasedLightFlicker.SetActive(true);
    }

    private IEnumerator WaitBeforeNextPick() {
        var ppc = PlayerPickupController.Instance;
        yield return new WaitUntil(() => ppc != null && !ppc.IsBusy);

        if (currentPickable >= 0 && currentPickable < pickables.Length) {
            ppc.TryPick(pickables[currentPickable]);
        }
    }

}
