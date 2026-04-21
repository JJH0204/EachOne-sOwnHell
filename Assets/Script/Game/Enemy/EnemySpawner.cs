using UnityEngine;

/// <summary>
/// 각자의 지옥 - 적 스폰 관리
///
/// 역할:
///   - 타이머·최대 수 기반으로 적을 스폰
///   - 스폰된 적에 EnemyController / BulletPatternEmitter / MonsterDrop 부착
///
/// 이벤트 버스:
///   구독 EnemyCountChangedEvent - 현재 적 수 추적
///   구독 GameOverEvent          - 게임 오버 시 스폰 중단
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public float spawnInterval = 5f;
    public int   maxEnemies   = 5;
    public float arenaRadius  = 12f;

    [Header("Drop Prefabs")]
    [SerializeField] private GameObject expOrbPrefab;
    [SerializeField] private GameObject itemPrefab;

    private int   _currentEnemies;
    private float _nextSpawnTime = 2f;
    private bool  _stopped;

    // ───────────────────────────────────────────────────────────
    void OnEnable()
    {
        EventBus<EnemyCountChangedEvent>.Subscribe(OnEnemyCountChanged);
        EventBus<GameOverEvent>.Subscribe(OnGameOver);
    }

    void OnDisable()
    {
        EventBus<EnemyCountChangedEvent>.Unsubscribe(OnEnemyCountChanged);
        EventBus<GameOverEvent>.Unsubscribe(OnGameOver);
    }

    void OnEnemyCountChanged(EnemyCountChangedEvent evt) => _currentEnemies = evt.Count;
    void OnGameOver(GameOverEvent _) => _stopped = true;

    // ───────────────────────────────────────────────────────────
    void Update()
    {
        if (_stopped) return;
        if (Time.time < _nextSpawnTime) return;
        if (_currentEnemies >= maxEnemies) return;

        SpawnEnemy();
        _nextSpawnTime = Time.time + spawnInterval;
    }

    // ─── 스폰 ──────────────────────────────────────────────────
    void SpawnEnemy()
    {
        Vector2 rim      = Random.insideUnitCircle.normalized * (arenaRadius * 0.88f);
        Vector3 spawnPos = new Vector3(rim.x, 0f, rim.y);

        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = "Enemy";
        go.tag  = "Enemy";
        go.transform.position   = spawnPos;
        go.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);

        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity     = false;
        rb.interpolation  = RigidbodyInterpolation.Interpolate;
        rb.constraints    = RigidbodyConstraints.FreezePositionY
                          | RigidbodyConstraints.FreezeRotationX
                          | RigidbodyConstraints.FreezeRotationZ;

        go.AddComponent<EnemyController>();

        var emitter   = go.AddComponent<BulletPatternEmitter>();
        emitter.pattern = (BulletPatternEmitter.PatternType)Random.Range(0, 3);

        var drop = go.AddComponent<MonsterDrop>();
        drop.Setup(expOrbPrefab, itemPrefab, 0.3f);
    }
}