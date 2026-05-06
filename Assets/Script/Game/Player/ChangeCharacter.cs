using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 각자의 지옥 - 플레이어 캐릭터 변경 관리
///
/// 역할:
///   - 플레이어 캐릭터 변경을 처리합니다
///   - 플레이어가 선택 한 캐릭터로 변경 할 수 있게끔 선택하지 않은 캐릭터가 나오지 않게끔 처리합니다
///   - 원하는 캐릭터로 변경 할때 변경 성공시 서브 스킬이 출력되도록 처리합니다
///   - 플레이어가 원하는 캐릭터로 이미 바꿧을 경우 한번 더 바뀌는걸 막도록 처리합니다
/// TODO:
///   - 프로토타입 진입 후 추가되는 내용에 따라 ( 현 스크립트는 중요도3으로 지정) 중요도3중 최우선이 되거나 중요도3 에서 후 순위가 됩니다
///   - 추후 캐릭터의 데이터가 추가 될때마다 추가 작성 할 예정 ( 현재 목업 버전이며 프로토타입에 맞게 현 스크립트를 변경 할 예정 )
/// </summary>

public class ChangeCharacter : MonoBehaviour
{
    [Header("Character")]
    public GameObject ada;
    public GameObject testPlayer;

    [SerializeField] private CinemachineCamera vcam;

    private InputAction _button1;
    private InputAction _button2;

    private void Awake()
    {
        var asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        var playerMap = asset.FindActionMap("Player", throwIfNotFound: true);
        _button1 = playerMap.FindAction("Previous", throwIfNotFound: true);
        _button2 = playerMap.FindAction("Next",     throwIfNotFound: true);
    }

    private void Start()
    {
        if (ada.activeSelf)
        {
            vcam.Target.TrackingTarget = ada.transform;
            vcam.Target.LookAtTarget   = ada.transform;
        }
        else if (testPlayer.activeSelf)
        {
            vcam.Target.TrackingTarget = testPlayer.transform;
            vcam.Target.LookAtTarget   = testPlayer.transform;
        }
    }

    private void OnEnable()
    {
        _button1.performed += Bt1;
        _button2.performed += Bt2;
        _button1.Enable();
        _button2.Enable();
    }

    private void OnDisable()
    {
        _button1.performed -= Bt1;
        _button2.performed -= Bt2;
        _button1.Disable();
        _button2.Disable();
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
        vcam.Target.LookAtTarget   = testPlayer.transform;
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
        vcam.Target.LookAtTarget   = ada.transform;
    }
}
