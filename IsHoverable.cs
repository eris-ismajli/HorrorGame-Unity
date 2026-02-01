using UnityEngine;

public abstract class IsHoverable : MonoBehaviour {
    [Header("Interaction")]
    public Transform player;
    [SerializeField] private float maxInteractionDistance = 1f;
    [SerializeField] protected string objectHoverInfo;

    public bool isInteractable { get; protected set; } = true;

    // Expose so the raycaster can check range
    public float MaxInteractionDistance => maxInteractionDistance;

    // Called by the raycaster when left mouse is pressed while this is hovered & in range
    public void ClickFromRaycaster() {
        HandleMouseDown();
    }

    // Your subclasses override this
    protected virtual void HandleMouseDown() { }

    private void OnMouseUp() {
        HandleMouseUp();
    }

    protected virtual void HandleMouseUp() { }

    public string ObjectHoverInfo => objectHoverInfo;

}
