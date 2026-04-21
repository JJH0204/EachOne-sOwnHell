using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class TestEnterTheArena : MonoBehaviour
{

    [FormerlySerializedAs("Enter")] [SerializeField] private InputAction _enter;
    
    private void OnEnable()
    {
        _enter.performed += EnterKey;
    
        _enter.Enable();
    }

    private void OnDisable()
    {
        _enter.performed -= EnterKey;
        _enter.Disable();
    }

    private void EnterKey(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("Arena");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("�÷��̾� ������");
    }
}
