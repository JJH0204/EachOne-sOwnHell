using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerDeath : MonoBehaviour
{
    public enum Difficulty
    {
        A, B
    }

    [Header("난이도")]
    public Difficulty difficulty = Difficulty.A;

    [Header("죽음 상태")]
    public bool isDead = false;

    [Header("사망시 키 입력")]
    public InputAction retryKey;
    public InputAction EndGameKey;

    [Header("막을 스크립트")]
    public PlayerController moveScript;
    public test_AutoAim autoAttackScript;

    [Header("B 난이도 타이머")]
    [SerializeField] private float bEndingUnlockTime = 60f;
    [SerializeField] private Coroutine bTimerCoroutine;

    private PlayerStats playerStats;

    private bool bTimerStarted = false;
    private bool bEndingUnlocked = false;

    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }

    private void Start()
    {
        if (difficulty == Difficulty.B)
        {
            StartBTimerRoutine();
        }
        isDead = false;
    }

    private void Update()
    {
        Debug.Log(bEndingUnlocked);
    }

    IEnumerator StartBTimer()
    {
        yield return new WaitForSeconds(bEndingUnlockTime);

        if (isDead)
        {
            yield break;
        }

        bEndingUnlocked = true;
        bTimerCoroutine = null;
    }

    public void HandleDeath()
    {
        if (isDead) return;

        isDead = true;

        if (moveScript != null)
            moveScript.enabled = false;

        if (autoAttackScript != null)
            autoAttackScript.enabled = false;

        if (bTimerCoroutine != null)
        {
            StopCoroutine(bTimerCoroutine);
            bTimerCoroutine = null;
        }

        bTimerStarted = false;

        // A 난이도면 죽자마자 바로 엔딩
        if (difficulty == Difficulty.A)
        {
            SceneManager.LoadScene("Test_EndingA");
            return;
        }

        // B 난이도는 UI 띄우고 선택지 제공
    }

    void ContinueB()
    {
        isDead = false;

        if (playerStats != null)
            playerStats.ReviveForMockup(100);

        if (moveScript != null)
            moveScript.enabled = true;

        if (autoAttackScript != null)
            autoAttackScript.enabled = true;

        if (difficulty == Difficulty.B && !bEndingUnlocked)
        {
            StartBTimerRoutine();
        }
    }

    private void OnEnable()
    {
        retryKey.performed += retryBtn;
        EndGameKey.performed += EndBtn;

        retryKey.Enable();
        EndGameKey.Enable();
    }

    private void OnDisable()
    {
        retryKey.performed -= retryBtn;
        EndGameKey.performed -= EndBtn;

        retryKey.Disable();
        EndGameKey.Disable();
    }

    void retryBtn(InputAction.CallbackContext context)
    {
        if (!isDead) return;

        if (difficulty == Difficulty.B)
        {
            ContinueB();
        }
    }

    void EndBtn(InputAction.CallbackContext context)
    {
        if (!isDead) return;

        if (difficulty != Difficulty.B) return;
        if (!bEndingUnlocked) return;

        SceneManager.LoadScene("Test_EndingB");
    }

    void StartBTimerRoutine()
    {
        // 혹시 이미 돌고 있으면 먼저 끊기
        if (bTimerCoroutine != null)
        {
            StopCoroutine(bTimerCoroutine);
        }

        // 상태 초기화
        bTimerStarted = true;
        bEndingUnlocked = false;

        bTimerCoroutine = StartCoroutine(StartBTimer());
    }

    // ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ 엔딩 GUI ㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡㅡ

    private void OnGUI()
    {
        if (!isDead) return;
        if (difficulty == Difficulty.A) return;

        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.fontSize = 40;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;
        titleStyle.normal.textColor = Color.red;

        GUIStyle textStyle = new GUIStyle(GUI.skin.label);
        textStyle.fontSize = 20;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.normal.textColor = Color.white;

        GUI.Label(new Rect(0, 120, Screen.width, 50), "GAME OVER", titleStyle);

        string optionText = "R : 부활";

        if (difficulty == Difficulty.B && bEndingUnlocked)
        {
            optionText = "R : 부활 / F : 엔딩";
        }

        GUI.Label(new Rect(0, 220, Screen.width, 40), "난이도 : " + difficulty, textStyle);
        GUI.Label(new Rect(0, 260, Screen.width, 40), optionText, textStyle);
    }
}