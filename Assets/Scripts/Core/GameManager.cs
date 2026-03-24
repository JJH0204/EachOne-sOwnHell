using System.Collections;
using UnityEngine;

/// <summary>
/// 각자의 지옥 - 게임 매니저 (싱글턴)
///
/// 역할:
///   - 적 스폰 관리 (시간·수 기반)
///   - 점수 집계
///   - 게임 오버 처리
///   - OnGUI로 그레이박스 HUD 표시
///     (HP 바, 스트레스 바, 점수, 상태 텍스트)
///
/// TODO (수직 슬라이스 단계):
///   - 로그라이트 런 관리
///   - 공명수치(Resonance) + 조율(Calibration) 시스템
///   - 아이템 드롭 풀
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Spawn Settings")]
    public float enemySpawnInterval = 5f;
    public int   maxEnemies         = 5;
    public float arenaRadius        = 12f;

    public bool IsGameOver { get; private set; }

    // ─── 내부 상태 ─────────────────────────────────────────────
    private int   score;
    private int   currentEnemies;
    private float nextSpawnTime = 2f;
    private string statusMessage;
    private float  statusExpireTime;

    // ─── HUD 레이아웃 상수 ────────────────────────────────────
    const float BAR_X  = 16f;
    const float BAR_Y  = 16f;
    const float BAR_W  = 220f;
    const float BAR_H  = 22f;
    const float BAR_GAP = 30f;

    // ─── PlayerStats 참조 캐시 ─────────────────────────────────
    private PlayerStats playerStats;

    // ───────────────────────────────────────────────────────────
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerStats = playerObj.GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (IsGameOver) return;

        if (Time.time >= nextSpawnTime && currentEnemies < maxEnemies)
        {
            SpawnEnemy();
            nextSpawnTime = Time.time + enemySpawnInterval;
        }

        // R 키 : 재시작
        if (UnityEngine.InputSystem.Keyboard.current?.rKey.wasPressedThisFrame == true && IsGameOver)
            RestartScene();
    }

    // ─── 적 스폰 ───────────────────────────────────────────────
    void SpawnEnemy()
    {
        // 아레나 가장자리에서 랜덤 위치 선택
        Vector2 rim     = Random.insideUnitCircle.normalized * (arenaRadius * 0.88f);
        Vector3 spawnPos = new Vector3(rim.x, 0f, rim.y);

        // 큐브 프리미티브로 적 생성
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Enemy";
        go.transform.position   = spawnPos;
        go.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);

        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity   = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints  = RigidbodyConstraints.FreezePositionY
                        | RigidbodyConstraints.FreezeRotationX
                        | RigidbodyConstraints.FreezeRotationZ;

        var enemy   = go.AddComponent<EnemyController>();
        var emitter = go.AddComponent<BulletPatternEmitter>();

        // 랜덤 패턴 선택 (다양성)
        emitter.pattern = (BulletPatternEmitter.PatternType)Random.Range(0, 3);

        currentEnemies++;
        enemy.onDeath += () => currentEnemies--;
    }

    // ─── 점수 ──────────────────────────────────────────────────
    public void AddScore(int amount)
    {
        score += amount;
    }

    // ─── 상태 메시지 ───────────────────────────────────────────
    public void PostStatus(string msg, float duration = 2.5f)
    {
        statusMessage    = msg;
        statusExpireTime = Time.time + duration;
    }

    // ─── 게임 오버 ─────────────────────────────────────────────
    public void TriggerGameOver()
    {
        IsGameOver = true;
    }

    void RestartScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex);
    }

    // ─── OnGUI HUD ─────────────────────────────────────────────
    void OnGUI()
    {
        DrawHUD();
        if (IsGameOver) DrawGameOver();
    }

    void DrawHUD()
    {
        if (playerStats == null) return;

        // 배경 패널
        GUI.color = new Color(0, 0, 0, 0.55f);
        GUI.Box(new Rect(BAR_X - 8, BAR_Y - 8, BAR_W + 16, BAR_H * 2 + BAR_GAP + 12), GUIContent.none);
        GUI.color = Color.white;

        // ── HP 바 ────────────────────────────────────────────
        float hpRatio = playerStats.currentHP / playerStats.maxHP;
        DrawBar(BAR_X, BAR_Y, BAR_W, BAR_H,
            hpRatio,
            new Color(0.2f, 0.8f, 0.2f),
            $"HP  {playerStats.currentHP:F0} / {playerStats.maxHP:F0}");

        // ── 스트레스 바 ──────────────────────────────────────
        float stressRatio = playerStats.currentStress / playerStats.maxStress;
        Color stressColor = Color.Lerp(new Color(0.3f, 0.5f, 1f), new Color(1f, 0.15f, 0.15f), stressRatio);
        string stressLabel = playerStats.IsIncapacitated ? "전투 불능!"
                           : playerStats.IsAwakened      ? "각성!!"
                           : $"STRESS  {playerStats.currentStress:F0} / {playerStats.maxStress:F0}";
        DrawBar(BAR_X, BAR_Y + BAR_GAP, BAR_W, BAR_H, stressRatio, stressColor, stressLabel);

        // ── 점수 ─────────────────────────────────────────────
        GUI.color = Color.white;
        GUI.Label(new Rect(BAR_X, BAR_Y + BAR_GAP * 2 + 4, 300, 28),
            $"SCORE  {score:N0}");

        // ── 상태 메시지 ───────────────────────────────────────
        if (!string.IsNullOrEmpty(statusMessage) && Time.time < statusExpireTime)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 22,
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
            style.normal.textColor = Color.yellow;
            GUI.Label(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 40, 320, 50),
                statusMessage, style);
        }

        // ── 조작 안내 (우측 상단) ────────────────────────────
        GUI.color = new Color(1, 1, 1, 0.6f);
        GUI.Label(new Rect(Screen.width - 220, 10, 210, 90),
            "WASD : 이동\nLMB  : 사격\n적 처치 : 점수 획득\n스트레스 MAX → 전투 불능 → 각성");
        GUI.color = Color.white;
    }

    void DrawBar(float x, float y, float w, float h, float ratio, Color fillColor, string label)
    {
        // 배경
        GUI.color = new Color(0.15f, 0.15f, 0.15f, 1f);
        GUI.DrawTexture(new Rect(x, y, w, h), Texture2D.whiteTexture);

        // 채워진 부분
        GUI.color = fillColor;
        GUI.DrawTexture(new Rect(x, y, w * Mathf.Clamp01(ratio), h), Texture2D.whiteTexture);

        // 레이블
        GUI.color = Color.white;
        GUIStyle s = new GUIStyle(GUI.skin.label) { fontSize = 12, alignment = TextAnchor.MiddleLeft };
        GUI.Label(new Rect(x + 4, y, w - 4, h), label, s);
    }

    void DrawGameOver()
    {
        // 어두운 오버레이
        GUI.color = new Color(0, 0, 0, 0.7f);
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
        GUI.color = Color.white;

        GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 42,
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        titleStyle.normal.textColor = new Color(0.9f, 0.3f, 0.3f);

        GUIStyle subStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize  = 20,
            alignment = TextAnchor.MiddleCenter
        };
        subStyle.normal.textColor = Color.white;

        float cx = Screen.width / 2f;
        float cy = Screen.height / 2f;

        GUI.Label(new Rect(cx - 200, cy - 80, 400, 70), "GAME OVER", titleStyle);
        GUI.Label(new Rect(cx - 200, cy,       400, 40), $"FINAL SCORE : {score:N0}", subStyle);
        GUI.Label(new Rect(cx - 200, cy + 50,  400, 30), "R 키 : 재시작", subStyle);
    }
}
