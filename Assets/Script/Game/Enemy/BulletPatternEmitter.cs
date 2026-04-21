using System.Collections;
using UnityEngine;


public class BulletPatternEmitter : MonoBehaviour
{
    public enum PatternType { Circular, Spiral, Aimed }

    [Header("Pattern")]
    public PatternType pattern       = PatternType.Circular;
    public int         bulletCount   = 8;
    public float       bulletSpeed   = 5.5f;
    public float       fireInterval  = 1.4f;

    private bool      _isActive;
    private Coroutine _patternRoutine;
    private Transform _player;
    private float     _spiralAngle;

    // ───────────────────────────────────────────────────────────
    void Start()
    {
        var go = GameObject.FindGameObjectWithTag("Player");
        if (go != null) _player = go.transform;
    }

    public void StartPattern()
    {
        if (_isActive) return;
        _isActive       = true;
        _patternRoutine = StartCoroutine(PatternLoop());
    }

    public void StopPattern()
    {
        if (!_isActive) return;
        _isActive = false;
        if (_patternRoutine != null) StopCoroutine(_patternRoutine);
    }

    // ─── 패턴 루프 ─────────────────────────────────────────────
    IEnumerator PatternLoop()
    {
        // 첫 발사 전 짧은 대기 (공격 진입 직후 즉발 방지)
        yield return new WaitForSeconds(0.4f);

        while (_isActive)
        {
            switch (pattern)
            {
                case PatternType.Circular: FireCircular(); break;
                case PatternType.Spiral:   FireSpiral();   break;
                case PatternType.Aimed:    FireAimed();    break;
            }
            yield return new WaitForSeconds(fireInterval);
        }
    }

    // ─── 원형 탄막 ─────────────────────────────────────────────
    void FireCircular()
    {
        float step = 360f / bulletCount;
        for (int i = 0; i < bulletCount; i++)
        {
            Vector3 dir = Quaternion.Euler(0, step * i, 0) * Vector3.forward;
            SpawnBullet(dir);
        }
    }

    // ─── 나선형 탄막 ───────────────────────────────────────────
    void FireSpiral()
    {
        int burst = 4;
        for (int i = 0; i < burst; i++)
        {
            float angle = _spiralAngle + (360f / burst) * i;
            Vector3 dir = Quaternion.Euler(0, angle, 0) * Vector3.forward;
            SpawnBullet(dir);
        }
        _spiralAngle = (_spiralAngle + 25f) % 360f;
    }

    // ─── 조준형 탄막 (3-way) ───────────────────────────────────
    void FireAimed()
    {
        if (_player == null) return;

        var baseDir = _player.position - transform.position;
        baseDir.y = 0f;
        if (baseDir.sqrMagnitude < 0.01f) return;
        baseDir.Normalize();

        const int ways = 3;
        const float spread = 18f;
        for (var i = 0; i < ways; i++)
        {
            var offset = (i - ways / 2) * spread;
            var dir  = Quaternion.Euler(0, offset, 0) * baseDir;
            SpawnBullet(dir);
        }
    }

    // ─── 탄환 생성 ─────────────────────────────────────────────
    private void SpawnBullet(Vector3 direction)
    {
        // NavMesh로 인해 강제 보정된 Y 좌표를 고정
        Vector3 origin = transform.position + Vector3.up * 0.6f;
        origin.y = 0.5f;

        EventBus<SpawnBulletRequestEvent>.Raise(
            new SpawnBulletRequestEvent(origin, direction, bulletSpeed,
                isPlayerBullet: false, isAutoAim: false));
    }
}
