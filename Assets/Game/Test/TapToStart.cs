using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class TapToStart : MonoBehaviour
{
    public string nextSceneName = "SampleScene";

    void Update()
    {
        // 마우스 클릭
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            StartGame();
        }

        // 터치
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            StartGame();
        }
    }

    void StartGame()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}