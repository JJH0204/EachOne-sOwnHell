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

    [Header("���̵�")]
    public Difficulty difficulty = Difficulty.A;

    [Header("���� ����")]
    public bool isDead;

    [Header("����� Ű �Է�")]
    public InputAction RetryKey;
    public InputAction EndGameKey;

    [Header("���� ��ũ��Ʈ")]
    public PlayerController moveScript;
    public TestAutoAim autoAttackScript;

    [Header("B ���̵� Ÿ�̸�")]
    [SerializeField] private float bEndingUnlockTime = 60f;
    private Coroutine _bTimerCoroutine;

    private PlayerStats _playerStats;

    private bool _bTimerStarted;
    private bool _bEndingUnlocked;

    private void Awake()
    {
        _playerStats = GetComponent<PlayerStats>();
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
        Debug.Log(_bEndingUnlocked);
    }

    private IEnumerator StartBTimer()
    {
        yield return new WaitForSeconds(bEndingUnlockTime);

        if (isDead)
        {
            yield break;
        }

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

        // A ���̵��� ���ڸ��� �ٷ� ����
        if (difficulty != Difficulty.A) return;
        SceneManager.LoadScene("Test_EndingA");

        // B ���̵��� UI ���� ������ ����
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
        {
            StartBTimerRoutine();
        }
    }

    private void OnEnable()
    {
        RetryKey.performed += RetryBtn;
        EndGameKey.performed += EndBtn;
    
        RetryKey.Enable();
        EndGameKey.Enable();
    }
    
    private void OnDisable()
    {
        RetryKey.performed -= RetryBtn;
        EndGameKey.performed -= EndBtn;
    
        RetryKey.Disable();
        EndGameKey.Disable();
    }

    private void RetryBtn(InputAction.CallbackContext context)
    {
        if (!isDead) return;
    
        if (difficulty == Difficulty.B)
        {
            ContinueB();
        }
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
        // Ȥ�� �̹� ���� ������ ���� ����
        if (_bTimerCoroutine != null)
        {
            StopCoroutine(_bTimerCoroutine);
        }

        // ���� �ʱ�ȭ
        _bTimerStarted = true;
        _bEndingUnlocked = false;

        _bTimerCoroutine = StartCoroutine(StartBTimer());
    }

    // �ѤѤѤѤѤѤѤѤѤѤѤ� ���� GUI �ѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤѤ�

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
            normal =
            {
                textColor = Color.red
            }
        };

        var textStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = 20,
            alignment = TextAnchor.MiddleCenter,
            normal =
            {
                textColor = Color.white
            }
        };

        GUI.Label(new Rect(0, 120, Screen.width, 50), "GAME OVER", titleStyle);

        var optionText = "R : ��Ȱ";

        if (difficulty == Difficulty.B && _bEndingUnlocked)
        {
            optionText = "R : ��Ȱ / F : ����";
        }

        GUI.Label(new Rect(0, 220, Screen.width, 40), "���̵� : " + difficulty, textStyle);
        GUI.Label(new Rect(0, 260, Screen.width, 40), optionText, textStyle);
    }
}