using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.InputSystem;
using Unity.VisualScripting;

public class Test_EnterTheArena : MonoBehaviour
{

    [SerializeField] InputAction Enter;

    private void OnEnable()
    {
        Enter.performed += EnterKey;

        Enter.Enable();
    }

    private void OnDisable()
    {
        Enter.performed -= EnterKey;
        Enter.Disable();
    }

    void EnterKey(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("Arena");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        Debug.Log("플레이어 감지됨");
    }

}
