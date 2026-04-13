using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ChangeCharacter : MonoBehaviour
{

    [Header("Character")]
    public GameObject Ada;
    public GameObject TestPlayer;

    [Header("ChangeButton")]
    public InputAction button1;
    public InputAction button2;


    public CinemachineVirtualCameraBase vcam;


    void Start()
    {
        if (Ada.activeSelf)
        {
            vcam.Follow = Ada.transform;
            vcam.LookAt = Ada.transform;
        }
        else if (TestPlayer.activeSelf)
        {
            vcam.Follow = TestPlayer.transform;
            vcam.LookAt = TestPlayer.transform;
        }
    }

    void OnEnable()
    {
        button1.performed += bt1;
        button2.performed += bt2;

        button1.Enable();
        button2.Enable();

    }

    public void OnDisable()
    {
        button1.performed -= bt1;
        button2.performed -= bt2;

        button1.Disable();
        button2.Disable();
    }


    void bt1(InputAction.CallbackContext context)
    {

        if (TestPlayer.activeSelf)
        {
            Debug.Log("이미 테스트플레이어 캐릭터 입니다");
        }
        else
        {
            Debug.Log("테스트 플레이어 교체 완료");
        }

        Ada.SetActive(false);
        TestPlayer.SetActive(true);

        vcam.Follow = TestPlayer.transform;
        vcam.LookAt = TestPlayer.transform;

    }


    void bt2(InputAction.CallbackContext context)
    {
        if (Ada.activeSelf)
        {
            Debug.Log("이미 에이다 캐릭터 입니다");
        }
        else
        {
            Debug.Log("에이다 교체 완료");
        }

        Ada.SetActive(true);
        TestPlayer.SetActive(false);

        vcam.Follow = Ada.transform;
        vcam.LookAt = Ada.transform;
    }

}
