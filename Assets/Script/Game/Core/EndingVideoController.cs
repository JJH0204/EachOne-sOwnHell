using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.InputSystem;

public class EndingVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string videoFileName = "ending.mp4";
    [SerializeField] private string nextSceneName = "LobbyScene";
    [SerializeField] InputAction Skip;



    // ─── 목업용 엔딩 스킵 버튼 ─────────────────────────────────────────────

    private void OnEnable()
    {
        Skip.performed += SkipBtn;
        Skip.Enable();
    }

    private void OnDisable()
    {
        Skip.performed -= SkipBtn;
        Skip.Disable();
    }

    void SkipBtn(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene(nextSceneName);
    }


    // ─── 엔딩 영상 ─────────────────────────────────────────────

    void Start()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();

        string videoPath = Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoPath;
        videoPlayer.playOnAwake = false;

        videoPlayer.loopPointReached += OnVideoFinished;
        videoPlayer.Play();
    }

    void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(nextSceneName);
    }

    void OnDestroy()
    {
        if (videoPlayer != null)
            videoPlayer.loopPointReached -= OnVideoFinished;
    }
}