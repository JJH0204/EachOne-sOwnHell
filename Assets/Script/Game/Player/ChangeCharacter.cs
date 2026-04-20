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

    public TestDrawGUI GUI;

    void Start()
    {

        if (!GUI) { Debug.Log("���ٴµ���?"); }

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
            Debug.Log("�̹� �׽�Ʈ�÷��̾� ĳ���� �Դϴ�");
        }
        else
        {
            GUI.is_Ada = false;
            Debug.Log("�׽�Ʈ �÷��̾� ��ü �Ϸ�");
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
            Debug.Log("�̹� ���̴� ĳ���� �Դϴ�");
        }
        else
        {
            GUI.is_Ada = true;
            Debug.Log("���̴� ��ü �Ϸ�");
        }

        Ada.SetActive(true);
        TestPlayer.SetActive(false);

        vcam.Follow = Ada.transform;
        vcam.LookAt = Ada.transform;
    }

}
