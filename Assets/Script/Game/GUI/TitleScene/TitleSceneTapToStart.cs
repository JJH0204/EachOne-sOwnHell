using System.Linq;
using UnityEngine;

/// <summary>
/// 각자의 지옥 - 씬 관리
///
/// 역할:
///   - 씬 이동을 처리합니다
///   - 다른 스크립트에서 씬 이동을 적지 않도록 여기서 관리 되게끔 합니다
/// TODO:
///   - 프로토타입 진입 후 추가되는 내용에 따라 ( 현 스크립트는 중요도3으로 지정) 중요도3중 최우선이 되거나 중요도3 에서 후 순위가 됩니다 )
///   - 추후 씬 매니저로 스크립트 변경 예정 ( 현재 목업 버전이며 프로토타입에 맞게 현 스크립트를 변경 할 예정 )
/// </summary>
///

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
