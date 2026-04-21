using UnityEngine;
using UnityEngine.Serialization;

public class ItemOrb : MonoBehaviour
{
    [FormerlySerializedAs("Item")] [SerializeField] private int item = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.transform.root.CompareTag("Player")) return;

        EventBus<ItemOrbPickedUpEvent>.Raise(new ItemOrbPickedUpEvent(item));
        Destroy(gameObject);
    }
}