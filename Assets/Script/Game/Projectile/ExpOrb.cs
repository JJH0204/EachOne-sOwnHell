using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;

public class ExpOrb : MonoBehaviour
{
    [SerializeField] private int expAmount = 1;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStats playerExp = other.GetComponent<PlayerStats>();

     if (playerExp != null)
        {
            playerExp.AddExp(expAmount);
            Destroy(gameObject);
        }
    }
}