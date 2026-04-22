using System.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif
#if HAS_ADDRESSABLES
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
#endif
using UnityEngine.SceneManagement;

public class TitleSceneTapToStart : MonoBehaviour
{
  [SerializeField] private string nextSceneKey = "LobbyScene";

  private bool _isLoading;

  private void Update()
  {
    if (_isLoading)
    {
      return;
    }

    if (IsStartInputTriggered())
    {
      StartTransition();
    }
  }

  private static bool IsStartInputTriggered()
  {
#if ENABLE_INPUT_SYSTEM
    if (Touchscreen.current != null)
    {
      if (Touchscreen.current.touches.Any(touch => touch.press.wasPressedThisFrame))
      {
        return true;
      }
    }

    if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
    {
      return true;
    }

    return Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
#else
        if (Input.GetMouseButtonDown(0))
        {
            return true;
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            return true;
        }

        return Input.anyKeyDown;
#endif
  }

  private void StartTransition()
  {
    if (string.IsNullOrWhiteSpace(nextSceneKey))
    {
      Debug.LogError("Next scene key is empty.", this);
      return;
    }

    _isLoading = true;

#if HAS_ADDRESSABLES
    var handle = Addressables.LoadSceneAsync(nextSceneKey);
    handle.Completed += OnSceneLoadCompleted;
#else
        SceneManager.LoadScene(nextSceneKey, LoadSceneMode.Single);
#endif
  }

#if HAS_ADDRESSABLES
  private void OnSceneLoadCompleted(AsyncOperationHandle<SceneInstance> handle)
  {
    if (handle.Status == AsyncOperationStatus.Succeeded)
    {
      return;
    }

    _isLoading = false;
    Debug.LogError($"Failed to load Addressable scene: {nextSceneKey}", this);
  }
#endif
}
