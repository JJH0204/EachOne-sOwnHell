using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class TestEnterTheArena : MonoBehaviour
{

    [SerializeField] private InputAction enter;

    private void OnEnable()
    {
        enter.performed += EnterKey;

        enter.Enable();
    }

    private void OnDisable()
    {
        enter.performed -= EnterKey;
        enter.Disable();
    }

    private static void EnterKey(InputAction.CallbackContext context)
    {
        Addressables.LoadSceneAsync("Arena");
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            Debug.Log("�÷��̾� ������");
    }
}
