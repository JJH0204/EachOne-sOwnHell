using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    public enum Difficulty { A, B }

    [Header("난이도")]
    public Difficulty difficulty = Difficulty.A;

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

    private void Update()
    {
        Debug.Log(_bEndingUnlocked);
    }

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
}