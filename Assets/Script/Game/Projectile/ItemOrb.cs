using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class ItemOrb : MonoBehaviour
{
    [SerializeField] private int Item = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStats playerExp = other.GetComponent<PlayerStats>();

     if (playerExp != null)
        {
            playerExp.AddItem(Item);
            Destroy(gameObject);
        }
    }
}