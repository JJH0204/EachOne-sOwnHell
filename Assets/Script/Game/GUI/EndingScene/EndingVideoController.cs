using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using System.IO;
// using UnityEngine.InputSystem;

public class EndingVideoController : MonoBehaviour
{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private string videoFileName = "ending.mp4";
    [SerializeField] private string nextSceneName = "LobbyScene";
    // [SerializeField] private InputAction _skip;



    // ������ ����� ���� ��ŵ ��ư ������������������������������������������������������������������������������������������

    // private void OnEnable()
    // {
    //     _skip.performed += SkipBtn;
    //     _skip.Enable();
    // }
    //
    // private void OnDisable()
    // {
    //     _skip.performed -= SkipBtn;
    //     _skip.Disable();
    // }
    //
    // void SkipBtn(InputAction.CallbackContext context)
    // {
    //     SceneManager.LoadScene(nextSceneName);
    // }


    // ������ ���� ���� ������������������������������������������������������������������������������������������

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