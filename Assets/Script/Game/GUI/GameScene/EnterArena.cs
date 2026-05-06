using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

//TODO : 프로토타입에선 사용 안될것이라 판단 사용처가 있다면 프토토타입에 맞게 개선 그게 아니라면 삭제 예정

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
            Debug.Log("아레나 진입 완료");
    }
}
