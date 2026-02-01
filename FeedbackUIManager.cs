using TMPro;
using UnityEngine;
using System.Collections;
using System;

public class FeedbackUIManager : MonoBehaviour {
    public static FeedbackUIManager Instance { get; private set; }

    public event EventHandler OnHoverReactivate;

    [Header("UI")]
    [SerializeField] private GameObject errorUI;
    [SerializeField] private TextMeshProUGUI errorText;

    private static readonly WaitForSeconds ErrorDelay = new WaitForSeconds(3f);
    private Coroutine errorRoutine;

    public bool feedbackIsActive = false;

    private void Awake() {
        Instance = this;

        errorUI.SetActive(true);

        if (errorText != null) {
            if (errorText.text == null || errorText.text.Length == 0) errorText.text = " ";
            errorText.ForceMeshUpdate();
            _ = errorText.renderedWidth;
        }

        Canvas.ForceUpdateCanvases();

        errorUI.SetActive(false);
    }

    public void ShowFeedback(string message) {
        errorText.text = message;

        if (errorRoutine != null) {
            StopCoroutine(errorRoutine);
        }
        errorRoutine = StartCoroutine(StartFeedback());
    }

    public void ShowCustomDelayedFeedback(string message, float delay) {
        errorText.text = message;

        if (errorRoutine != null) {
            StopCoroutine(errorRoutine);
        }

        errorRoutine = StartCoroutine(StartCustomLengthFeedback(delay));
    }

    private IEnumerator StartCustomLengthFeedback(float delay) {
        feedbackIsActive = true;
        errorUI.SetActive(true);
        yield return new WaitForSeconds(delay);
        errorUI.SetActive(false);
        feedbackIsActive = false;
        OnHoverReactivate?.Invoke(this, EventArgs.Empty);
        errorRoutine = null;
    }

    private IEnumerator StartFeedback() {
        feedbackIsActive = true;
        errorUI.SetActive(true);
        yield return ErrorDelay;
        errorUI.SetActive(false);
        feedbackIsActive = false;
        OnHoverReactivate?.Invoke(this, EventArgs.Empty);
        errorRoutine = null;
    }
}
