using UnityEngine;
using System.Collections;
using System;

public class KeyAnimator : MonoBehaviour {
    public static event EventHandler OnKeyTwist;

    [SerializeField] private Transform startPos;
    [SerializeField] private float moveDuration = 2f;
    [SerializeField] private float spinDuration = 2f;
    [SerializeField] private float spinDirection; // e.g. 180 or -180 (total degrees)

    [Header("Pull out")]
    [SerializeField] private float waitAfterTurn = 1f;
    [SerializeField] private float pullOutDuration = 0.6f;
    [SerializeField] private float pullOutDistanceZ = 0.15f; // keyhole local +Z distance to pull out

    public Coroutine InsertAndTurn(Transform keyhole) {
        StopAllCoroutines();
        transform.SetParent(null, true);

        // place at your start
        transform.position = startPos.position;

        // inherit keyhole rotation
        transform.rotation = keyhole.rotation;

        return StartCoroutine(AnimateKey(keyhole));
    }

    private IEnumerator AnimateKey(Transform keyhole) {
        Vector3 from = transform.position;
        Vector3 to = keyhole.position;

        Quaternion startR = transform.rotation;
        Quaternion endR = keyhole.rotation;

        // -------- Insert --------
        float t = 0f;
        while (t < moveDuration) {
            t += Time.deltaTime;
            float f = t / moveDuration;

            transform.position = Vector3.Lerp(from, to, f);
            transform.rotation = Quaternion.Slerp(startR, endR, f);

            yield return null;
        }

        transform.position = to;
        transform.rotation = endR;

        // -------- Turn --------
        OnKeyTwist?.Invoke(this, EventArgs.Empty);

        t = 0f;
        while (t < spinDuration) {
            float dt = Time.deltaTime;
            t += dt;

            // spinDirection is TOTAL degrees over the whole spinDuration
            float step = spinDirection * (dt / spinDuration);

            // rotate around keyhole axis (more stable than transform.forward)
            transform.Rotate(keyhole.forward, step, Space.World);

            yield return null;
        }

        // -------- Wait --------
        if (waitAfterTurn > 0f)
            yield return new WaitForSeconds(waitAfterTurn);

        // -------- Pull out along keyhole local +Z --------
        Vector3 pullFrom = transform.position;

        // keyhole local +Z moves "out" in your setup
        Vector3 pullTo = keyhole.TransformPoint(0f, 0f, pullOutDistanceZ);

        Quaternion pullStartR = transform.rotation;
        Quaternion pullEndR = transform.rotation; // keep the turned rotation while pulling out

        t = 0f;
        while (t < pullOutDuration) {
            t += Time.deltaTime;
            float f = t / pullOutDuration;

            transform.position = Vector3.Lerp(pullFrom, pullTo, f);
            transform.rotation = Quaternion.Slerp(pullStartR, pullEndR, f);

            yield return null;
        }

        transform.position = pullTo;

        Destroy(gameObject);
    }
}
