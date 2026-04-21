using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class ChangeCharacter : MonoBehaviour
{

    [FormerlySerializedAs("Ada")] [Header("Character")]
    public GameObject ada;
    public GameObject testPlayer;

    [Header("ChangeButton")]
    public InputAction button1;
    public InputAction button2;


    [FormerlySerializedAs("_vcam")] [SerializeField] private CinemachineCamera vcam;

    private void Start()
    {
        if (ada.activeSelf)
        {
            vcam.Target.TrackingTarget = ada.transform;
            vcam.Target.LookAtTarget = ada.transform;
        }
        else if (testPlayer.activeSelf)
        {
            vcam.Target.TrackingTarget = testPlayer.transform;
            vcam.Target.LookAtTarget = testPlayer.transform;
        }
    }

    private void OnEnable()
    {
        button1.performed += Bt1;
        button2.performed += Bt2;

        button1.Enable();
        button2.Enable();

    }

    private void OnDisable()
    {
        button1.performed -= Bt1;
        button2.performed -= Bt2;

        button1.Disable();
        button2.Disable();
    }


    private void Bt1(InputAction.CallbackContext context)
    {
        if (testPlayer.activeSelf)
        {
            Debug.Log("이미 테스트플레이어 캐릭터 입니다");
        }
        else
        {
            EventBus<CharacterChangedEvent>.Raise(new CharacterChangedEvent(false));
            Debug.Log("테스트 플레이어 교체 완료");
        }

        ada.SetActive(false);
        testPlayer.SetActive(true);

        vcam.Target.TrackingTarget = testPlayer.transform;
        vcam.Target.LookAtTarget = testPlayer.transform;

    }


    private void Bt2(InputAction.CallbackContext context)
    {
        if (ada.activeSelf)
        {
            Debug.Log("이미 아다인 캐릭터 입니다");
        }
        else
        {
            EventBus<CharacterChangedEvent>.Raise(new CharacterChangedEvent(true));
            Debug.Log("아다인 교체 완료");
        }

        ada.SetActive(true);
        testPlayer.SetActive(false);

        vcam.Target.TrackingTarget = ada.transform;
        vcam.Target.LookAtTarget = ada.transform;
    }

}