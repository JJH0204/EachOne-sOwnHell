using UnityEngine;

/// <summary>
/// 각자의 지옥 - 기본 적 컨트롤러 (FSM 기반)
///
/// 상태 머신:
///   Idle   → (플레이어 감지 범위 진입) → Chase
///   Chase  → (공격 범위 진입)          → Attack
///   Attack → (공격 범위 이탈)          → Chase
///   Any    → (HP = 0)                  → Dead
///
/// 기획서 4.2: "FSM 기반의 간단한 행동 패턴을 가진 기본 몬스터"
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    // ─── 전역 카운터 (PlayerStats 스트레스 계산용) ─────────────
    public static int ActiveCount { get; private set; }

    // ─── FSM 상태 ───────────────────────────────────────────────
    public enum State { Idle, Chase, Attack, Dead }

    [Header("Stats")]
    public float hp         = 30f;
    public float moveSpeed  = 2.8f;
    public int   scoreValue = 100;

    [Header("Detection")]
    public float detectRange = 11f;
    public float attackRange = 6.5f;

    // ─── 이벤트 ────────────────────────────────────────────────
    public event System.Action onDeath;

    // ─── 색상 상수 ─────────────────────────────────────────────
    static readonly Color ColorIdle   = new Color(0.55f, 0.20f, 0.20f);
    static readonly Color ColorChase  = new Color(0.85f, 0.30f, 0.10f);
    static readonly Color ColorAttack = new Color(1.00f, 0.10f, 0.10f);
    static readonly Color ColorDead   = new Color(0.30f, 0.30f, 0.30f);

    // ─── 내부 ──────────────────────────────────────────────────
    private State                 state = State.Idle;
    private Rigidbody             rb;
    private BulletPatternEmitter  emitter;
    private Renderer              rend;
    private Transform             player;

    // ───────────────────────────────────────────────────────────
    void Awake()  => ActiveCount++;
    void OnDestroy()
    {
        // Dead() 이외 경로로 소멸될 때 카운터 보정
        if (state != State.Dead) ActiveCount--;
    }

    void Start()
    {
        rb      = GetComponent<Rigidbody>();
        emitter = GetComponent<BulletPatternEmitter>();
        rend    = GetComponentInChildren<Renderer>();

        rb.useGravity  = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        SetColor(ColorIdle);
    }

    void Update()
    {
        if (state == State.Dead || player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        UpdateFSM(dist);
    }

    void FixedUpdate()
    {
        if (state != State.Chase || player == null) return;

        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.01f)
        {
            dir.Normalize();
            rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(Quaternion.LookRotation(dir));
        }
    }

    // ─── FSM 전이 ──────────────────────────────────────────────
    void UpdateFSM(float dist)
    {
        switch (state)
        {
            case State.Idle:
                if (dist < detectRange) TransitionTo(State.Chase);
                break;

            case State.Chase:
                if (dist < attackRange)          TransitionTo(State.Attack);
                else if (dist > detectRange * 1.2f) TransitionTo(State.Idle);
                break;

            case State.Attack:
                if (dist > attackRange * 1.2f) TransitionTo(State.Chase);
                // 공격 중 플레이어 방향 유지
                Vector3 lookDir = player.position - transform.position;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0.01f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
                break;
        }
    }

    void TransitionTo(State next)
    {
        state = next;
        switch (next)
        {
            case State.Idle:
                emitter?.StopPattern();
                SetColor(ColorIdle);
                break;
            case State.Chase:
                emitter?.StopPattern();
                SetColor(ColorChase);
                break;
            case State.Attack:
                emitter?.StartPattern();
                SetColor(ColorAttack);
                break;
        }
    }

    // ─── 피해 / 사망 ───────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (state == State.Dead) return;
        hp -= amount;
        if (hp <= 0f) Dead();
    }

    void Dead()
    {
        state = State.Dead;
        ActiveCount--;
        emitter?.StopPattern();
        SetColor(ColorDead);
        onDeath?.Invoke();
        GameManager.Instance?.AddScore(scoreValue);
        Destroy(gameObject, 0.2f);
    }

    void SetColor(Color c)
    {
        if (rend != null) rend.material.color = c;
    }
}
