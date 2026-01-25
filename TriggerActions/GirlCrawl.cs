using System.Collections;
using UnityEngine;

public class GirlCrawl : MonoBehaviour {
    [SerializeField] private LightManager kitchenLight;
    [SerializeField] private GameObject creepyGirl;
    [SerializeField] private Transform playerCamera;
    [SerializeField] private float rayDistance = 2f;

    [Header("Animator")]
    [SerializeField] private string crawlStateName = "Crawl"; // exact state name in layer 0

    [SerializeField] private Transform passiveGirlPos;

    private Animator girlAnim;
    private Transform camT;
    private bool triggered;

    private void Awake() {
        camT = playerCamera != null ? playerCamera : Camera.main.transform;

        girlAnim = creepyGirl.GetComponent<Animator>();

    }

    private void Update() {
        if (triggered) return;

        if (Physics.Raycast(camT.position, camT.forward, out RaycastHit hit, rayDistance)) {
            if (hit.collider.gameObject == gameObject) {
                triggered = true;
                StartCoroutine(CrawlJumpscareCo());
            }
        }
    }

    private IEnumerator CrawlJumpscareCo() {
        // Light break first (cheap path)
        if (kitchenLight.IsThisLightOn()) {
            kitchenLight.BreakLight();
            SoundManager.Instance.PlayLightbulbBreakSound1(camT.position, 0.8f);
        }
        else {
            kitchenLight.BreakLight();
        }

        //  PREP ANIM FULLY WHILE STILL INVISIBLE 
        // Jump instantly into the crawl state at t=0 (no transition)
        girlAnim.ResetTrigger(crawlStateName);
        girlAnim.CrossFadeInFixedTime(crawlStateName, 0f, 0, 0f);

        // Force evaluation so bones snap to the first frame *this* frame
        // Two zero-updates guarantees state change then pose application.
        girlAnim.Update(0f);
        girlAnim.Update(0f);

        // If the clip has root motion that would move the rig on its first frame,
        // you can optionally freeze root motion just for this prep tick:
        // bool prevRM = girlAnim.applyRootMotion; girlAnim.applyRootMotion = false;
        // girlAnim.Update(0f); girlAnim.applyRootMotion = prevRM;

        SoundManager.Instance.PlayCrawlingSound(creepyGirl.transform.position, 1f);

        // Small buffer, then remove the trigger object
        yield return null;
        Destroy(gameObject);
    }
}
