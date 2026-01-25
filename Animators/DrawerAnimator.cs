using System;
using UnityEngine;

public class DrawerAnimator : IsHoverable {
    public static event EventHandler<OnDrawerChangedEventArgs> OnDrawerChanged;

    public class OnDrawerChangedEventArgs : EventArgs {
        public bool isOpen;
    }

    private Animator animator;
    private BoxCollider drawerCollider;
    private bool isOpen = false;

    // This boolean flips to false when the player interacts
    // then it flips back to true when the animation is done
    // (prevents unecessary bugs caused by spamming)
    private bool canInteract = true;

    public bool drawerHasObject = false;

    private void Awake() {
        animator = GetComponent<Animator>();
        drawerCollider = GetComponent<BoxCollider>();
    }

    protected override void HandleMouseDown() {
        if (!canInteract) return;
        canInteract = false;
        isOpen = !isOpen;

        OnDrawerChanged?.Invoke(this, new OnDrawerChangedEventArgs { isOpen = isOpen });

        animator.SetTrigger(isOpen ? "DrawerOpen" : "DrawerClose");
    }

    public void DisableCollider() {
        if (!drawerHasObject)
            return;
        drawerCollider.enabled = false;
    }

    public void AllowDrawerInteraction() {
        canInteract = true;
    }
}
