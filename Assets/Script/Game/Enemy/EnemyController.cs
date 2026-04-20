using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 각자의 지옥 - 기본 적 컨트롤러 (FSM 기반)
///
/// 상태 머신:
///   Idle   → (플레이어 감지 범위 진입) → Chase
///   Chase  → (공격 범위 진입)          → Attack
///   Attack → (공격 범위 이탈)          → Chase
///   Any    → (HP = 0)                  → Dead
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    // ─── 전역 카운터 ───────────────────────────────────────────
    public static int ActiveCount { get; private set; }

    // ─── FSM 상태 ───────────────────────────────────────────────
    public enum State { Idle, Chase, Attack, Dead }

    [Header("Stats")]
    public float hp = 30f;
    public float moveSpeed = 3.5f;
    public int scoreValue = 100;

    [Header("Detection")]
    public float detectRange = 11f;
    public float attackRange = 6.5f;

    public float lastHitTime;
    public int damage = 10;

    public MonsterDrop drop;

    // ─── 이벤트 ────────────────────────────────────────────────
    public event System.Action onDeath;

    // ─── 색상 상수 ─────────────────────────────────────────────
    static readonly Color ColorIdle = new Color(0.55f, 0.20f, 0.20f);
    static readonly Color ColorChase = new Color(0.85f, 0.30f, 0.10f);
    static readonly Color ColorAttack = new Color(1.00f, 0.10f, 0.10f);
    static readonly Color ColorDead = new Color(0.30f, 0.30f, 0.30f);

    // ─── 내부 ──────────────────────────────────────────────────
    private State state = State.Idle;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private BulletPatternEmitter emitter;
    private Renderer rend;
    private Transform player;

    // ───────────────────────────────────────────────────────────
    void Awake()
    {
        ActiveCount++;
        agent = GetComponent<NavMeshAgent>();
    }

    void OnDestroy()
    {
        if (state != State.Dead)
            ActiveCount--;
    }

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        emitter = GetComponent<BulletPatternEmitter>();
        rend = GetComponentInChildren<Renderer>();

        drop = GetComponent<MonsterDrop>();

        // Rigidbody 기본 설정
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezePositionY
                       | RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // NavMeshAgent 기본 설정
        agent.speed = moveSpeed;
        agent.angularSpeed = 360f;
        agent.stoppingDistance = attackRange * 0.9f;
        agent.acceleration = 20f;
        agent.updateRotation = true;
        agent.updateUpAxis = false;

        // 플레이어 찾기
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        SetColor(ColorIdle);
    }

    void Update()
    {
        if (state == State.Dead || player == null)
            return;

        float dist = Vector3.Distance(transform.position, player.position);

        UpdateFSM(dist);

        switch (state)
        {
            case State.Chase:
                UpdateChase();
                break;

            case State.Attack:
                UpdateAttack();
                break;
        }
    }

    // ─── FSM 전이 판단 ─────────────────────────────────────────
    void UpdateFSM(float dist)
    {
        switch (state)
        {
            case State.Idle:
                if (dist < detectRange)
                    TransitionTo(State.Chase);
                break;

            case State.Chase:
                if (dist < attackRange)
                    TransitionTo(State.Attack);
                else if (dist > detectRange * 1.2f)
                    TransitionTo(State.Idle);
                break;

            case State.Attack:
                if (dist > attackRange * 1.2f)
                    TransitionTo(State.Chase);
                break;
        }
    }

    // ─── 상태 실행: Chase ──────────────────────────────────────
    void UpdateChase()
    {
        if (!agent.isOnNavMesh || player == null)
            return;

        agent.SetDestination(player.position);
    }

    // ─── 상태 실행: Attack ─────────────────────────────────────
    void UpdateAttack()
    {
        // 공격 중에는 이동 멈추고 플레이어 방향만 유지
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    // ─── 상태 전이 ─────────────────────────────────────────────
    void TransitionTo(State next)
    {
        if (state == next)
            return;

        state = next;

        switch (next)
        {
            case State.Idle:
                if (agent.isOnNavMesh)
                    agent.ResetPath();

                emitter?.StopPattern();
                SetColor(ColorIdle);
                break;

            case State.Chase:
                emitter?.StopPattern();
                SetColor(ColorChase);
                break;

            case State.Attack:
                if (agent.isOnNavMesh)
                    agent.ResetPath();

                emitter?.StartPattern();
                SetColor(ColorAttack);
                break;
        }
    }

    // ─── 피해 / 사망 ───────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (state == State.Dead)
            return;

        hp -= amount;

        if (hp <= 0f)
            Dead();
    }

    void Dead()
    {
        state = State.Dead;
        ActiveCount--;

        if (drop != null)
        {
            drop.DropItems();
        }

        if (agent != null && agent.isOnNavMesh)
            agent.ResetPath();

        emitter?.StopPattern();
        SetColor(ColorDead);
        onDeath?.Invoke();

        GameManager.Instance?.AddScore(scoreValue);

        Destroy(gameObject, 0.2f);

    }

    void SetColor(Color c)
    {
        if (rend != null)
            rend.material.color = c;
    }

    // ─── 충돌 판정 ( 플레이어 -> 몬스터 ) ────────────────────────────────────

    void OnTriggerStay(Collider other)
    {
        if (Time.time - lastHitTime < 1f) return;

        var stats = other.GetComponentInParent<PlayerStats>();
        if (stats == null) return;

        stats.TakeDamage(damage);
        lastHitTime = Time.time;
    }



}