using UnityEngine;
using UnityEngine.SceneManagement;
// using UnityEngine.InputSystem;

public class TitleSceneTapToStart : MonoBehaviour
{
    public string nextSceneName = "Arena";

    private void Update()
    {
        // // ���콺 Ŭ��
        // if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     StartGame();
        // }
        //
        // // ��ġ
        // if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        // {
        //     StartGame();
        // }
    }

    private void StartGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}