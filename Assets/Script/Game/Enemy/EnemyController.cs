using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    // ─── 전역 카운터 ───────────────────────────────────────────
    private static int ActiveCount { get; set; }

    // ─── FSM 상태 ───────────────────────────────────────────────
    private enum State { Idle, Chase, Attack, Dead }

    [Header("Stats")]
    public float hp = 30f;
    public float maxHp = 30f;
    public float moveSpeed = 3.5f;
    public int scoreValue = 100;

    [Header("Detection")]
    public float detectRange = 11f;
    public float attackRange = 6.5f;

    public float lastHitTime;
    public int damage = 10;

    public MonsterDrop drop;

    // ─── 색상 상수 ─────────────────────────────────────────────
    static readonly Color ColorIdle = new Color(0.55f, 0.20f, 0.20f);
    static readonly Color ColorChase = new Color(0.85f, 0.30f, 0.10f);
    static readonly Color ColorAttack = new Color(1.00f, 0.10f, 0.10f);
    static readonly Color ColorDead = new Color(0.30f, 0.30f, 0.30f);

    // ─── 내부 ──────────────────────────────────────────────────
    private State _state = State.Idle;

    private Rigidbody _rb;
    private NavMeshAgent _agent;
    private BulletPatternEmitter _emitter;
    private Renderer _rend;
    private Transform _player;

    // ───────────────────────────────────────────────────────────
    private void Awake()
    {
        ActiveCount++;
        _agent = GetComponent<NavMeshAgent>();
        EventBus<EnemyCountChangedEvent>.Raise(new EnemyCountChangedEvent(ActiveCount));
        EventBus<EnemyRegisteredEvent>.Raise(new EnemyRegisteredEvent(gameObject));
    }

    private void OnEnable()  => EventBus<BulletHitEnemyEvent>.Subscribe(OnBulletHit);
    public void OnDisable() => EventBus<BulletHitEnemyEvent>.Unsubscribe(OnBulletHit);

    private void OnBulletHit(BulletHitEnemyEvent evt)
    {
        if (evt.Target != gameObject) return;
        TakeDamage(evt.Damage);
    }

    private void OnDestroy()
    {
        if (_state != State.Dead)
        {
            ActiveCount--;
            EventBus<EnemyUnregisteredEvent>.Raise(new EnemyUnregisteredEvent(gameObject));
            EventBus<EnemyCountChangedEvent>.Raise(new EnemyCountChangedEvent(ActiveCount));
        }
    }

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _emitter = GetComponent<BulletPatternEmitter>();
        _rend = GetComponentInChildren<Renderer>();

        drop = GetComponent<MonsterDrop>();
        maxHp = hp;

        // Rigidbody 기본 설정
        _rb.useGravity = false;
        _rb.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationX;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;

        // NavMeshAgent 기본 설정
        _agent.speed = moveSpeed;
        _agent.angularSpeed = 360f;
        _agent.stoppingDistance = attackRange * 0.9f;
        _agent.acceleration = 20f;
        _agent.updateRotation = true;
        _agent.updateUpAxis = false;

        // 플레이어 찾기
        var playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            _player = playerObj.transform;

        SetColor(ColorIdle);
    }

    private void Update()
    {
        if (_state == State.Dead || _player == null)
            return;

        var dist = Vector3.Distance(transform.position, _player.position);

        UpdateFsm(dist);

        switch (_state)
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
    private void UpdateFsm(float dist)
    {
        switch (_state)
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
        if (!_agent.isOnNavMesh || _player == null)
            return;

        _agent.SetDestination(_player.position);
    }

    // ─── 상태 실행: Attack ─────────────────────────────────────
    void UpdateAttack()
    {
        // 공격 중에는 이동 멈추고 플레이어 방향만 유지
        Vector3 lookDir = _player.position - transform.position;
        lookDir.y = 0f;

        if (lookDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(lookDir);
    }

    // ─── 상태 전이 ─────────────────────────────────────────────
    void TransitionTo(State next)
    {
        if (_state == next)
            return;

        _state = next;

        switch (next)
        {
            case State.Idle:
                if (_agent.isOnNavMesh)
                    _agent.ResetPath();

                _emitter?.StopPattern();
                SetColor(ColorIdle);
                break;

            case State.Chase:
                _emitter?.StopPattern();
                SetColor(ColorChase);
                break;

            case State.Attack:
                if (_agent.isOnNavMesh)
                    _agent.ResetPath();

                _emitter?.StartPattern();
                SetColor(ColorAttack);
                break;
        }
    }

    // ─── 피해 / 사망 ───────────────────────────────────────────
    public void TakeDamage(float amount)
    {
        if (_state == State.Dead)
            return;

        hp -= amount;
        EventBus<EnemyHpChangedEvent>.Raise(new EnemyHpChangedEvent(gameObject, Mathf.Clamp01(hp / maxHp)));

        if (hp <= 0f)
            Dead();
    }

    void Dead()
    {
        _state = State.Dead;
        ActiveCount--;

        if (drop != null)
            drop.DropItems();

        if (_agent != null && _agent.isOnNavMesh)
            _agent.ResetPath();

        _emitter?.StopPattern();
        SetColor(ColorDead);

        EventBus<EnemyUnregisteredEvent>.Raise(new EnemyUnregisteredEvent(gameObject));
        EventBus<EnemyCountChangedEvent>.Raise(new EnemyCountChangedEvent(ActiveCount));
        EventBus<EnemyDiedEvent>.Raise(new EnemyDiedEvent(scoreValue));

        Destroy(gameObject, 0.2f);
    }

    void SetColor(Color c)
    {
        if (_rend != null)
            _rend.material.color = c;
    }

    // ─── 충돌 판정 ( 플레이어 -> 몬스터 ) ────────────────────────────────────
    void OnTriggerStay(Collider other)
    {
        if (Time.time - lastHitTime < 1f) return;
        if (!other.transform.root.CompareTag("Player")) return;

        EventBus<EnemyContactDamageEvent>.Raise(new EnemyContactDamageEvent(damage));
        lastHitTime = Time.time;
    }



}