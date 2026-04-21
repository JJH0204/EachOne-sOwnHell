using UnityEngine;

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private int expAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        EventBus<ExpOrbPickedUpEvent>.Raise(new ExpOrbPickedUpEvent(expAmount));
        Destroy(gameObject);
    }
}