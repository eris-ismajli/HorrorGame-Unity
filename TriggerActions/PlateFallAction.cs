using UnityEngine;

public class PlateFallAction : MonoBehaviour {

    public static PlateFallAction Instance { get; private set; }

    [SerializeField] private GameObject[] platePieces;
    [SerializeField] private GameObject plate;


    private Animator plateAnim;
    private Rigidbody pieceRb;
    private MeshRenderer plateMeshRenderer;

    private bool released;

    void Awake() {
        Instance = this;

        plateAnim = plate.GetComponent<Animator>();
        plateMeshRenderer = plate.GetComponent<MeshRenderer>();

        // Animate while kinematic

        foreach (GameObject piece in platePieces) {
            pieceRb = piece.GetComponent<Rigidbody>();
            pieceRb.isKinematic = true;
            pieceRb.useGravity = false;
            pieceRb.interpolation = RigidbodyInterpolation.Interpolate;
            pieceRb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        }

    }

    // IMPORTANT: This function gets called through the plate animation event
    public void ReleasePlate() {
        if (released) return;
        released = true;

        plateMeshRenderer.enabled = false;
        // Stop any parent motion and Animator influence
        plate.transform.SetParent(null, true);
        plateAnim.enabled = false;


        foreach (GameObject piece in platePieces) {
            pieceRb = piece.GetComponent<Rigidbody>();
            pieceRb.isKinematic = false;
            pieceRb.useGravity = true;
            pieceRb.AddForce(transform.forward * .7f, ForceMode.Impulse);
        }

        Destroy(gameObject);
    }

    private void TriggerPlateFall() {
        plateAnim.ResetTrigger("MovePlate");
        plateAnim.SetTrigger("MovePlate");
    }

    private bool triggered = false;

    private void OnTriggerEnter(Collider other) {
        if (triggered) return;

        if (other.CompareTag("Player")) {
            triggered = true;
            TriggerPlateFall();
        }
    }

}
