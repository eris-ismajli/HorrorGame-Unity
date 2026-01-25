using UnityEngine;

public class PlanksManager : MonoBehaviour {

    [SerializeField] private BreakPlank[] planks;
    [SerializeField] private GameObject animatedAxe;
    [SerializeField] private EquipableObjectSO equippedAxe;

    private int planksCounter;

    private void Start() {
        planksCounter = planks.Length;

        foreach (BreakPlank plank in planks) {
            plank.OnPlankBreak += Plank_OnPlankBreak;
        }
    }

    private void Plank_OnPlankBreak(object sender, System.EventArgs e) {
        planksCounter--;
        if (planksCounter == 0) {
            Destroy(animatedAxe);
            PlayerInventory.Instance.Unequip(equippedAxe);
            Destroy(gameObject);
        }
    }
}
