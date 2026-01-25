using System;
using UnityEngine;

public class DetectSafeClick : IsHoverable {

    public event EventHandler OnSafeClicked;
    protected override void HandleMouseDown() {
        OnSafeClicked?.Invoke(this, EventArgs.Empty);
    }
}
