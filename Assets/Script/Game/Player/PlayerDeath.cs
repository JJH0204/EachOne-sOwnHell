using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 각자의 지옥 - 플레이어 죽음 및 난이도 관리
///
/// 역할:
///   - 플레이어 죽음 관련을 처리합니다
///   - 플레이어 난이도를 처리 합니다
///   - 플레이어 사망시 난이도에 따라 결과를 바꿉니다
///   - A 난이도에서 특정 조건 없으며 사망 시 곧바로 A엔딩 영상이 출력 됩니다
///   - B 난이도에서 일정 시간후에 사망시 엔딩을 선택하거나 부활 할 기회를 얻습니다
/// 우선도:
///   - 추후 작업 우선 순위가 높은건 *** 이며 * 가 낮아질수록 우선도가 낮습니다
///   - GUI (우선도 3) : 프로토타입 때 사망 관련 작업할때 GUI 매니저로 옮길 예정 입니다
///   - 빈 공간 (우선도 1) : 현재 업데이트 함수에 내용이 없으며 추후 프로토 타입 끝날때 까지 작업이 안되었다면 삭제 할 예정
/// </summary>

public class PlayerDeath : MonoBehaviour
{
    public enum Difficulty { A, B }

    [Header("난이도")]
    private Difficulty difficulty = Difficulty.A;

    [Header("사망 상태")]
    public bool isDead;

    [Header("참조 스크립트")]
    public PlayerController moveScript;
    public TestAutoAim autoAttackScript;

    [Header("B 난이도 타이머")]
    [SerializeField] private float bEndingUnlockTime = 60f;
    private Coroutine _bTimerCoroutine;

    private InputAction _retryAction;
    private InputAction _endGameAction;

    private PlayerStats _playerStats;
    private bool _bTimerStarted;
    private bool _bEndingUnlocked;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();

        var asset = Resources.Load<InputActionAsset>("InputSystem_Actions");
        var playerMap = asset.FindActionMap("Player", throwIfNotFound: true);
        _retryAction   = playerMap.FindAction("Retry",   throwIfNotFound: true);
        _endGameAction = playerMap.FindAction("EndGame", throwIfNotFound: true);
    }

    private void Start()
    {
        if (difficulty == Difficulty.B)
            StartBTimerRoutine();

        isDead = false;
    }

  #region 빈공간 *
  /*    private void Update()
      {

      }*/
  #endregion

  private IEnumerator StartBTimer()
    {
        yield return new WaitForSeconds(bEndingUnlockTime);

        if (isDead) yield break;

        _bEndingUnlocked = true;
        _bTimerCoroutine = null;
    }

    public void HandleDeath()
    {
        if (isDead) return;

        isDead = true;

        if (moveScript)
            moveScript.enabled = false;

        if (autoAttackScript)
            autoAttackScript.enabled = false;

        if (_bTimerCoroutine != null)
        {
            StopCoroutine(_bTimerCoroutine);
            _bTimerCoroutine = null;
        }

        _bTimerStarted = false;

        if (difficulty != Difficulty.A) return;
        SceneManager.LoadScene("Test_EndingA");
    }

    private void ContinueB()
    {
        isDead = false;

        if (_playerStats != null)
            _playerStats.ReviveForMockup(100);

        if (moveScript != null)
            moveScript.enabled = true;

        if (autoAttackScript != null)
            autoAttackScript.enabled = true;

        if (difficulty == Difficulty.B && !_bEndingUnlocked)
            StartBTimerRoutine();
    }

    private void OnEnable()
    {
        _retryAction.performed   += RetryBtn;
        _endGameAction.performed += EndBtn;
        _retryAction.Enable();
        _endGameAction.Enable();
    }

    private void OnDisable()
    {
        _retryAction.performed   -= RetryBtn;
        _endGameAction.performed -= EndBtn;
        _retryAction.Disable();
        _endGameAction.Disable();
    }

    private void RetryBtn(InputAction.CallbackContext context)
    {
        if (!isDead) return;
        if (difficulty == Difficulty.B)
            ContinueB();
    }

    private void EndBtn(InputAction.CallbackContext context)
    {
        if (!isDead) return;
        if (difficulty != Difficulty.B) return;
        if (!_bEndingUnlocked) return;

        SceneManager.LoadScene("Test_EndingB");
    }

    private void StartBTimerRoutine()
    {
        if (_bTimerCoroutine != null)
            StopCoroutine(_bTimerCoroutine);

        _bTimerStarted   = true;
        _bEndingUnlocked = false;
        _bTimerCoroutine = StartCoroutine(StartBTimer());
    }

  #region 사망 출력 GUI ***
  private void OnGUI()
    {
        if (!isDead) return;
        if (difficulty == Difficulty.A) return;

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        var titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 40,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.red }
        };

        var textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = Color.white }
        };

        GUI.Label(new Rect(0, 120, Screen.width, 50), "GAME OVER", titleStyle);

        var optionText = "R : 부활";
        if (difficulty == Difficulty.B && _bEndingUnlocked)
            optionText = "R : 부활 / F : 엔딩";

        GUI.Label(new Rect(0, 220, Screen.width, 40), "난이도 : " + difficulty, textStyle);
        GUI.Label(new Rect(0, 260, Screen.width, 40), optionText, textStyle);
    }
  #endregion
}
